using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context;
using CocApi.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Cache.Services
{
    public sealed class NewCwlWarService : PerpetualService<NewCwlWarService>
    {
        internal event AsyncEventHandler<WarAddedEventArgs>? ClanWarAdded;


        internal ClansApi ClansApi { get; }
        internal Synchronizer Synchronizer { get; }
        internal TimeToLiveProvider Ttl { get; }
        internal IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }


        private Dictionary<DateTime, ConcurrentDictionary<string, SeenCwlWar>>? _downloadedWars;


        public NewCwlWarService(
            ILogger<NewCwlWarService> logger,
            IServiceScopeFactory scopeFactory,
            ClansApi clansApi, 
            Synchronizer synchronizer,
            TimeToLiveProvider ttl,
            IOptions<CacheOptions> options) 
        : base(logger, scopeFactory, options.Value.NewCwlWars.DelayBeforeExecution, options.Value.NewCwlWars.DelayBetweenExecutions)
        {
            Instantiated = Library.EnsureSingleton(Instantiated);
            IsEnabled = options.Value.NewCwlWars.Enabled;
            ClansApi = clansApi;
            Synchronizer = synchronizer;
            Ttl = ttl;
            Options = options;
        }


        private protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            _downloadedWars = await FillDownloadedWarsAsync(cancellationToken).ConfigureAwait(false);

            RemoveOldWars();

            ServiceOptions options = Options.Value.NewCwlWars;

            using var scope = ScopeFactory.CreateScope();

            CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    c.CurrentWar.Download &&
                    !c.Group.Added &&
                    !string.IsNullOrWhiteSpace(c.Group.RawContent) &&
                    c.Id > _id)
                .OrderBy(c => c.Id)
                .Take(options.ConcurrentUpdates)
                .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

            _id = cachedClans.Count == options.ConcurrentUpdates
                ? cachedClans.Max(c => c.Id)
                : int.MinValue;

            Dictionary<DateTime, Dictionary<string, Model.ClanWarLeagueGroup>> warTagsToDownload = new();

            List<Task<CachedWar>> announceNewWarTasks = new();

            HashSet<string> updatingTags = new();

            try
            {
                foreach (CachedClan cachedClan in cachedClans)
                {
                    if (!Synchronizer.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                        continue;

                    updatingTags.Add(cachedClan.Tag);

                    cachedClan.Group.Added = true;

                    _downloadedWars.TryAdd(cachedClan.Group.Content.Season, new ConcurrentDictionary<string, SeenCwlWar>());

                    var group = _downloadedWars.Single(w => w.Key == cachedClan.Group.Season);

                    foreach (var round in cachedClan.Group.Content.Rounds)
                        foreach (var warTag in round.WarTags.Where(w => w != "#0"))
                            if (group.Value.TryGetValue(warTag, out SeenCwlWar? seenCwlWar))
                            {
                                if (seenCwlWar.ApiResponse?.Content == null || (seenCwlWar.ClanTag != cachedClan.Tag && seenCwlWar.OpponentTag != cachedClan.Tag))
                                    continue; // if null we already announced it

                                CachedClan? clan = cachedClans.SingleOrDefault(c => c.Tag == seenCwlWar.ApiResponse.Content.Clan.Tag);

                                CachedClan? opponent = cachedClans.SingleOrDefault(c => c.Tag == seenCwlWar.ApiResponse.Content.Opponent.Tag);

                                announceNewWarTasks.Add(
                                    NewWarFoundAsync(
                                        clan?.Content, opponent?.Content, cachedClan.Group.Content, seenCwlWar.ApiResponse, seenCwlWar, cancellationToken));
                            }
                            else
                            {
                                warTagsToDownload.TryAdd(group.Key, new Dictionary<string, Model.ClanWarLeagueGroup>());

                                Dictionary<string, Model.ClanWarLeagueGroup> tags = warTagsToDownload.Single(w => w.Key == group.Key).Value;

                                if (!tags.ContainsKey(warTag))
                                    tags.Add(warTag, cachedClan.Group.Content);
                            }
                }

                List<Task> processRequests = new();

                foreach (KeyValuePair<DateTime, Dictionary<string, Model.ClanWarLeagueGroup>> warTagDictionaries in warTagsToDownload)
                    foreach (var warTags in warTagDictionaries.Value)
                        processRequests.Add(ProcessRequest(warTags, cachedClans, announceNewWarTasks, warTagDictionaries, cancellationToken));

                try
                {
                    await Task.WhenAll(processRequests).ConfigureAwait(false);

                    await Task.WhenAll(announceNewWarTasks).ConfigureAwait(false);
                }
                catch (Exception)
                {
                }

                foreach (var task in announceNewWarTasks.Where(t => t.IsCompletedSuccessfully))
                    dbContext.Wars.Add(task.Result);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingTags)
                    Synchronizer.UpdatingClan.TryRemove(tag, out _);
            }
        }

        private async Task<Dictionary<DateTime, ConcurrentDictionary<string, SeenCwlWar>>> FillDownloadedWarsAsync(CancellationToken cancellationToken)
        {
            if (_downloadedWars != null)
                return _downloadedWars;

            Dictionary<DateTime, ConcurrentDictionary<string, SeenCwlWar>> result = new();

            using var scope = ScopeFactory.CreateScope();

            CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

            DateTime since = DateTime.UtcNow.AddMonths(-2).AddDays(-DateTime.UtcNow.Day - 1); // go back to begining of month two months ago

            List<CachedWar> cachedWars = await dbContext.Wars
                .AsNoTracking()
                .Where(w =>
                    !string.IsNullOrWhiteSpace(w.WarTag) &&
                    !string.IsNullOrWhiteSpace(w.RawContent) &&
                    w.PreparationStartTime > since
                    )
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (CachedWar cachedWar in cachedWars)
            {
                result.TryAdd(cachedWar.Season.Value, new ConcurrentDictionary<string, SeenCwlWar>());

                var downloadedWars = result.Single(w => w.Key == cachedWar.Season.Value);

                SeenCwlWar seenWar = new(cachedWar.Season.Value, cachedWar.ClanTag, cachedWar.OpponentTag, cachedWar.WarTag, null);

                downloadedWars.Value.TryAdd(cachedWar.WarTag, seenWar);
            }

            return result;
        }

        private void RemoveOldWars()
        {
            DateTime since = DateTime.UtcNow.AddMonths(-2).AddDays(-DateTime.UtcNow.Day - 1); // go back to begining of month two months ago

            var warsToRemove = _downloadedWars.Keys.Where(k => k < since).ToList();

            warsToRemove.ForEach(k => _downloadedWars.Remove(k, out var _));
        }

        private async Task ProcessRequest(
            KeyValuePair<string, Model.ClanWarLeagueGroup> kvp, 
            List<CachedClan> cachedClans, 
            List<Task<CachedWar>> announceNewWarTasks, 
            KeyValuePair<DateTime, Dictionary<string, Model.ClanWarLeagueGroup>> warTags,
            CancellationToken cancellationToken)
        {
            try
            {
                ApiResponse<Model.ClanWar>? apiResponse = null;

                try
                {
                    apiResponse = await ClansApi.FetchClanWarLeagueWarResponseAsync(kvp.Key, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                }

                if (cancellationToken.IsCancellationRequested || apiResponse?.Content == null)
                    return;

                SeenCwlWar seenCwlWar = new(warTags.Key, apiResponse.Content.Clan.Tag, apiResponse.Content.Opponent.Tag, kvp.Key, apiResponse);

                var group = _downloadedWars.Single(w => w.Key == warTags.Key).Value;

                group.TryAdd(kvp.Key, seenCwlWar);

                CachedClan? cachedClan = cachedClans.SingleOrDefault(c => c.Tag == apiResponse.Content.Clan.Tag);

                CachedClan? cachedOpponent = cachedClans.SingleOrDefault(c => c.Tag == apiResponse.Content.Opponent.Tag);

                if (cachedClan != null || cachedOpponent != null)
                    announceNewWarTasks.Add(
                        NewWarFoundAsync(cachedClan?.Content, cachedOpponent?.Content, kvp.Value, apiResponse, seenCwlWar, cancellationToken));
            }
            catch (Exception e)
            {
                if (!cancellationToken.IsCancellationRequested)
                    Logger.LogError(e, "An exception occured while executing {0}.{1}().", GetType().Name, nameof(ProcessRequest));

                throw;
            }
        }

        private async Task<CachedWar> NewWarFoundAsync(
            Model.Clan? clan, 
            Model.Clan? opponent, 
            Model.ClanWarLeagueGroup group, 
            ApiResponse<CocApi.Model.ClanWar> war, 
            SeenCwlWar seenCwlWar,
            CancellationToken cancellationToken)
        {
            try
            {
                if (ClanWarAdded != null)
                    await ClanWarAdded.Invoke(this, new CwlWarAddedEventArgs(clan, opponent, war.Content, group, cancellationToken)).ConfigureAwait(false);

                TimeSpan timeToLive = await Ttl.TimeToLiveOrDefaultAsync(war).ConfigureAwait(false);

                CachedWar cachedWar = new(war, timeToLive, war.Content.WarTag, group.Season);

                seenCwlWar.ApiResponse = null; // dont announce this war again, also prevent memory leak

                return cachedWar;
            }
            catch (Exception e)
            {
                if (!cancellationToken.IsCancellationRequested)
                    Logger.LogError(e, "An exception occured while executing {0}.{1}().", GetType().Name, nameof(NewWarFoundAsync));

                throw;
            }
        }
    }
}