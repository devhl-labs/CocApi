using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    internal class NewCwlWarMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;
        private readonly IOptions<ClanMonitorsOptions> _options;
        private readonly Dictionary<DateTime, HashSet<string>> _downloadedWars = new();
        private readonly object _dbContextLock = new();

        public NewCwlWarMonitor(CacheDbContextFactoryProvider provider, ClansApi clansApi, ClansClientBase clansClient, IOptions<ClanMonitorsOptions> options) : base(provider)
        {
            _clansApi = clansApi;
            _clansClient = clansClient;
            _options = options;
        }

        protected override async Task PollAsync()
        {
            MonitorOptions options = _options.Value.NewCwlWars;

            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    c.CurrentWar.Download &&
                    !c.Group.Added &&
                    !string.IsNullOrWhiteSpace(c.Group.RawContent) &&
                    c.Id > _id)
                .OrderBy(c => c.Id)
                .Take(options.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
                .ConfigureAwait(false);

            _id = cachedClans.Count == options.ConcurrentUpdates
                ? cachedClans.Max(c => c.Id)
                : int.MinValue;

            List<Task<Client.ApiResponse<CocApi.Model.ClanWar>>> tasks = new();

            Dictionary<string, DateTime> updatingTags = new();

            try
            {
                foreach (CachedClan cachedClan in cachedClans)
                {
                    _downloadedWars.TryAdd(cachedClan.Group.Season.Value, new HashSet<string>());

                    HashSet<string> downloadedWars = _downloadedWars.Single(w => w.Key == cachedClan.Group.Season).Value;

                    foreach (var round in cachedClan.Group.Content.Rounds)
                        foreach (string tag in round.WarTags.Where(t => t != "#0" && !downloadedWars.Contains(t) && _clansClient.UpdatingCwlWar.TryAdd(t, null)))
                        {
                            updatingTags.Add(tag, cachedClan.Group.Season.Value);

                            tasks.Add(_clansApi.FetchClanWarLeagueWarResponseAsync(tag, _cancellationToken));
                        }
                }

                List<CachedWar> cachedWars = await dbContext.Wars
                    .Where(w => !string.IsNullOrWhiteSpace(w.WarTag) && updatingTags.Keys.Contains(w.WarTag))
                    .ToListAsync(_cancellationToken)
                    .ConfigureAwait(false);

                await Task.WhenAll(tasks);

                _cancellationToken.ThrowIfCancellationRequested();

                List<Client.ApiResponse<CocApi.Model.ClanWar>> allFetchedWars = new();

                foreach (var task in tasks.Where(t => t.Result is Client.ApiResponse<CocApi.Model.ClanWar>))
                    allFetchedWars.Add(task.Result);

                if (!allFetchedWars.Any())
                    return;

                List<Task> saveWars = new();

                foreach (CachedClan cachedClan in cachedClans)
                {
                    List<Client.ApiResponse<CocApi.Model.ClanWar>> wars = allFetchedWars
                        .Where(w => w.IsSuccessStatusCode && w.Content != null && w.Content.Clans.Any(c => c.Key == cachedClan.Tag)).ToList();

                    foreach (Client.ApiResponse<CocApi.Model.ClanWar> warResponse in wars)
                    {
                        if (cachedWars.Any((c => c.WarTag == warResponse.Content.WarTag && c.Season == cachedClan.Group.Season)))
                            continue;

                        saveWars.Add(NewWarFoundAsync(cachedClans, cachedClan, warResponse, dbContext));
                    }
                }

                await Task.WhenAll(saveWars);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

                foreach (CachedClan cachedClan in cachedClans)
                {
                    List<Client.ApiResponse<CocApi.Model.ClanWar>> wars = allFetchedWars
                        .Where(w => w.IsSuccessStatusCode && w.Content != null && w.Content.Clans.Any(c => c.Key == cachedClan.Tag)).ToList();

                    HashSet<string> downloadedWars = _downloadedWars.Single(w => w.Key == cachedClan.Group.Season).Value;

                    foreach (Client.ApiResponse<CocApi.Model.ClanWar> warResponse in wars)
                        downloadedWars.Add(warResponse.Content.WarTag);
                }

                if (_id == int.MinValue)
                    await Task.Delay(options.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
                else
                    await Task.Delay(options.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                foreach (var tag in updatingTags.Keys)
                {
                    _clansClient.UpdatingCwlWar.TryRemove(tag, out var _);
                }
            }
        }

        private async Task NewWarFoundAsync(List<CachedClan> cachedClans, CachedClan cachedClan, Client.ApiResponse<CocApi.Model.ClanWar> war, CacheDbContext dbContext)
        {
            CocApi.Model.Clan? clan = cachedClans.FirstOrDefault(c => c.Tag == war.Content.Clan.Tag)?.Content;

            CocApi.Model.Clan? opponent = cachedClans.FirstOrDefault(c => c.Tag == war.Content.Opponent.Tag)?.Content;

            await _clansClient.OnClanWarAddedAsync(new CwlWarAddedEventArgs(clan, opponent, war.Content, cachedClan.Group.Content, _cancellationToken)).ConfigureAwait(false);

            TimeSpan timeToLive = await _clansClient.TimeToLiveOrDefaultAsync(war).ConfigureAwait(false);

            CachedWar cachedWar = new(war, timeToLive, war.Content.WarTag, cachedClan.Group.Season.Value);

            lock (_dbContextLock)
                dbContext.Wars.Add(cachedWar);
        }
    }
}