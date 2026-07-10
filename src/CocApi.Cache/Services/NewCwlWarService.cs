using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.Apis;
using CocApi.Cache.Context;
using CocApi.Rest.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CocApi.Cache.Services.Options;

namespace CocApi.Cache.Services;

public sealed class NewCwlWarService : ServiceBase<NewCwlWarServiceOptions>
{
    internal event AsyncEventHandler<WarAddedEventArgs>? ClanWarAdded;

    public ILogger<NewCwlWarService> Logger { get; }

    internal IApiFactory ApiFactory { get; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider Ttl { get; }
    public IOptions<CacheOptions> CacheOptions { get; }
    internal IOptionsMonitor<NewCwlWarServiceOptions> NewCwlWarOptions { get; }
    internal static bool Instantiated { get; private set; }


    private Dictionary<DateTime, ConcurrentDictionary<string, SeenCwlWar>>? _downloadedSeasons;


    public NewCwlWarService(
        ILogger<NewCwlWarService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory, 
        Synchronizer synchronizer,
        TimeToLiveProvider ttl,
        IOptions<CacheOptions> cacheOptions,
        IOptionsMonitor<NewCwlWarServiceOptions> newCwlWarOptions,
        ILoggerFactory loggerFactory)
    : base(logger, scopeFactory, newCwlWarOptions, loggerFactory)
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        Logger = logger;
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        Ttl = ttl;
        CacheOptions = cacheOptions;
        NewCwlWarOptions = newCwlWarOptions;
    }


    protected override async Task<CycleCounters> ExecuteCycleAsync(CancellationToken cancellationToken)
    {
        _downloadedSeasons = await FillDownloadedWarsAsync(cancellationToken).ConfigureAwait(false);

        RemoveOldWars();

        NewCwlWarServiceOptions newCwlWarOptions = NewCwlWarOptions.CurrentValue;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedClan> cachedClans = await dbContext.Clans
            .Where(c =>
                c.CurrentWar.Download &&
                !c.Group.Added &&
                !string.IsNullOrWhiteSpace(c.Group.RawContent) &&
                c.Id > _id)
            .OrderBy(c => c.Id)
            .Take(newCwlWarOptions.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
        .ConfigureAwait(false);

        _id = cachedClans.Count == newCwlWarOptions.ConcurrentUpdates
            ? cachedClans.Max(c => c.Id)
            : int.MinValue;

        Dictionary<DateTime, Dictionary<string, Rest.Models.ClanWarLeagueGroup>> seasons = new();

        ConcurrentBag<Task<CachedWar>> announceNewWarTasks = new();

        ConcurrentDictionary<string, byte?> announcedWarTags = new();

        HashSet<string> updatingTags = new();
        int lockSkips = 0;
        long totalSaveMs = 0;

        IClansApi clansApi = ApiFactory.Create<IClansApi>();

        try
        {
            foreach (CachedClan cachedClan in cachedClans.Where(c => c.Group.Content != null)) // content will be null if the RawContent contains notInWar
            {
                if (!Synchronizer.ClanLock.TryAcquire(cachedClan.Tag))
                {
                    lockSkips++;
                    continue;
                }

                updatingTags.Add(cachedClan.Tag);

                cachedClan.Group.Added = true;

                _downloadedSeasons.TryAdd(cachedClan.Group.Content.Season, new ConcurrentDictionary<string, SeenCwlWar>());

                var group = _downloadedSeasons.Single(w => w.Key == cachedClan.Group.Season);

                foreach (var round in cachedClan.Group.Content.Rounds)
                    foreach (var warTag in round.WarTags.Where(w => w != "#0"))
                        if (group.Value.TryGetValue(warTag, out SeenCwlWar? seenCwlWar))
                        {
                            if (!announcedWarTags.TryAdd(warTag, null))
                                continue;

                            if (seenCwlWar.ApiResponse == null || !seenCwlWar.ApiResponse.TryOk(out Rest.Models.ClanWar? model) || (seenCwlWar.ClanTag != cachedClan.Tag && seenCwlWar.OpponentTag != cachedClan.Tag))
                                continue; // if null we already announced it

                            CachedClan? clan = cachedClans.SingleOrDefault(c => c.Tag == model.Clan.Tag);

                            CachedClan? opponent = cachedClans.SingleOrDefault(c => c.Tag == model.Opponent.Tag);

                            announceNewWarTasks.Add(
                                NewWarFoundAsync(
                                    clan?.Content, opponent?.Content, cachedClan.Group.Content, seenCwlWar.ApiResponse, cancellationToken));

                            seenCwlWar.ApiResponse = null; // dont announce this war again, also prevent memory leak
                        }
                        else
                        {
                            seasons.TryAdd(group.Key, new Dictionary<string, Rest.Models.ClanWarLeagueGroup>());

                            Dictionary<string, Rest.Models.ClanWarLeagueGroup> season = seasons.Single(w => w.Key == group.Key).Value;

                            season.TryAdd(warTag, cachedClan.Group.Content);
                        }
            }

            List<Task> processRequests = new();

            foreach (KeyValuePair<DateTime, Dictionary<string, Rest.Models.ClanWarLeagueGroup>> season in seasons)
                foreach (var warTags in season.Value)
                {
                    Option<bool> realTime = CacheOptions.Value.RealTime != null ? new Option<bool>(CacheOptions.Value.RealTime.Value) : default;
                    await Synchronizer.UpdateSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    processRequests.Add(Synchronizer.WithSemaphoreAsync(ProcessRequest(clansApi, realTime, announcedWarTags, warTags, cachedClans, announceNewWarTasks, season, cancellationToken)));
                }

            try
            {
                await Task.WhenAll(processRequests).WaitAsync(cancellationToken).ConfigureAwait(false);

                await Task.WhenAll(announceNewWarTasks).WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An exception occured while processing new cwl wars.");
            }

            var warsToAdd = announceNewWarTasks.Where(t => t.IsCompletedSuccessfully).Select(t => t.Result).ToList();

            foreach (var war in warsToAdd)
                dbContext.Wars.Add(war);

            var saveSw = System.Diagnostics.Stopwatch.StartNew();
            await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            totalSaveMs += saveSw.ElapsedMilliseconds;

            return new CycleCounters(
                cachedClans.Count,
                warsToAdd.Count,
                lockSkips,
                totalSaveMs);
        }
        finally
        {
            foreach (string tag in updatingTags)
                Synchronizer.ClanLock.Release(tag);
        }
    }

    private async Task<Dictionary<DateTime, ConcurrentDictionary<string, SeenCwlWar>>> FillDownloadedWarsAsync(CancellationToken cancellationToken)
    {
        if (_downloadedSeasons != null)
            return _downloadedSeasons;

        Dictionary<DateTime, ConcurrentDictionary<string, SeenCwlWar>> result = new();

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        DateTime since = DateTime.UtcNow.AddDays(-45); // get all cwl wars from the previous iteration

        List<CachedWar> cachedWars = await dbContext.Wars
            .AsNoTracking()
            .Where(w =>
                !string.IsNullOrWhiteSpace(w.WarTag) &&
                // !string.IsNullOrWhiteSpace(w.RawContent) &&
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

        var warsToRemove = _downloadedSeasons.Keys.Where(k => k < since).ToList();

        warsToRemove.ForEach(k => _downloadedSeasons.Remove(k, out var _));
    }

    private async Task ProcessRequest(
        IClansApi clansApi,
        Option<bool> realtime,
        ConcurrentDictionary<string, byte?> announcedWars,
        KeyValuePair<string, Rest.Models.ClanWarLeagueGroup> kvp,
        List<CachedClan> cachedClans,
        ConcurrentBag<Task<CachedWar>> announceNewWarTasks,
        KeyValuePair<DateTime, Dictionary<string, Rest.Models.ClanWarLeagueGroup>> warTags,
        CancellationToken cancellationToken)
    {
        try
        {
            IOk<Rest.Models.ClanWar?> apiResponse = await clansApi.FetchClanWarLeagueWarAsync(kvp.Key, realtime, cancellationToken).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested || !apiResponse.IsSuccessStatusCode || !apiResponse.TryOk(out Rest.Models.ClanWar? model))
                return;

            SeenCwlWar seenCwlWar = new(warTags.Key, model.Clan.Tag, model.Opponent.Tag, kvp.Key, apiResponse);

            var group = _downloadedSeasons.Single(w => w.Key == warTags.Key).Value;
            group.TryAdd(kvp.Key, seenCwlWar);

            CachedClan? cachedClan = cachedClans.SingleOrDefault(c => c.Tag == model.Clan.Tag);

            CachedClan? cachedOpponent = cachedClans.SingleOrDefault(c => c.Tag == model.Opponent.Tag);

            if ((cachedClan != null || cachedOpponent != null) && announcedWars.TryAdd(kvp.Key, null))
            {
                announceNewWarTasks.Add(
                    NewWarFoundAsync(cachedClan?.Content, cachedOpponent?.Content, kvp.Value, apiResponse, cancellationToken));

                seenCwlWar.ApiResponse = null; // dont announce this war again, also prevent memory leak
            }
        }
        catch (Exception e)
        {
            if (!cancellationToken.IsCancellationRequested)
                Logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", GetType().Name, nameof(ProcessRequest));

            throw;
        }
    }

    private async Task<CachedWar> NewWarFoundAsync(
        Rest.Models.Clan? clan,
        Rest.Models.Clan? opponent,
        Rest.Models.ClanWarLeagueGroup group,
        IOk<Rest.Models.ClanWar> war,
        CancellationToken cancellationToken)
    {
        try
        {
            Rest.Models.ClanWar? clanWar = war.Ok();

            if (ClanWarAdded != null)
                await ClanWarAdded.Invoke(this, new CwlWarAddedEventArgs(clan, opponent, clanWar, group, cancellationToken)).ConfigureAwait(false);

            TimeSpan timeToLive = await Ttl.TimeToLiveOrDefaultAsync(war).ConfigureAwait(false);

            CachedWar cachedWar = new(war, timeToLive, clanWar.WarTag, group.Season);

            return cachedWar;
        }
        catch (Exception e)
        {
            if (!cancellationToken.IsCancellationRequested)
                Logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", GetType().Name, nameof(NewWarFoundAsync));

            throw;
        }
    }
}