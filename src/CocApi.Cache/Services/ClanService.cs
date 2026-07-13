using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CocApi;
using CocApi.Rest.Apis;
using CocApi.Cache.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CocApi.Rest.Client;
using CocApi.Cache.Services.Options;
using CocApi.Cache.Logging;

namespace CocApi.Cache.Services;

public sealed class ClanService : ServiceBase<ClanServiceOptions>
{

    private readonly ILogger<ClanService> _logger;
    private readonly FireAndForgetService _fireAndForget;


    internal event AsyncEventHandler<ClanUpdatedEventArgs>? ClanUpdated;
    internal event AsyncEventHandler<ClanWarLeagueGroupUpdatedEventArgs>? ClanWarLeagueGroupUpdated;
    internal event AsyncEventHandler<ClanWarLogUpdatedEventArgs>? ClanWarLogUpdated;


    internal IApiFactory ApiFactory { get; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider Ttl { get; }
    internal IOptionsMonitor<CacheOptions> CacheOptions { get; }
    internal IOptionsMonitor<ClanServiceOptions> ClanOptions { get; }
    internal IOptionsMonitor<CwlWarServiceOptions> CwlWarOptions { get; }
    internal static bool Instantiated { get; private set; }

    public ClanService(
        ILogger<ClanService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory,
        Synchronizer synchronizer,
        TimeToLiveProvider ttl,
        IOptionsMonitor<CacheOptions> cacheOptions,
        IOptionsMonitor<ClanServiceOptions> clanOptions,
        IOptionsMonitor<CwlWarServiceOptions> cwlWarOptions,
        ILoggerFactory loggerFactory,
        FireAndForgetService fireAndForget)
        : base(logger, scopeFactory, clanOptions, loggerFactory)
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        Ttl = ttl;
        CacheOptions = cacheOptions;
        ClanOptions = clanOptions;
        CwlWarOptions = cwlWarOptions;
        _logger = logger;
        _fireAndForget = fireAndForget;
    }


    protected override async Task<CycleCounters> ExecuteCycleAsync(CancellationToken cancellationToken)
    {
        ClanServiceOptions clanOptions = ClanOptions.CurrentValue;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedClan> cachedClans = await dbContext.Clans
            .Where(c =>
                c.Id > _id &&
                (
                    (clanOptions.DownloadClan && (c.ExpiresAt ?? min) < expires && (c.KeepUntil ?? min) < now && c.Download) ||
                    (clanOptions.DownloadGroup && (c.Group.ExpiresAt ?? min) < expires && (c.Group.KeepUntil ?? min) < now && c.Group.Download) ||
                    // (clanOptions.DownloadCurrentWar && (c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < now && c.CurrentWar.Download && c.IsWarLogPublic != false) ||
                    (clanOptions.DownloadWarLog && (c.WarLog.ExpiresAt ?? min) < expires && (c.WarLog.KeepUntil ?? min) < now && c.WarLog.Download && c.IsWarLogPublic != false)
                ))
            .OrderBy(c => c.Id)
            .Take(clanOptions.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _id = cachedClans.Count == clanOptions.ConcurrentUpdates
            ? cachedClans.Max(c => c.Id)
            : int.MinValue;

        HashSet<string> updatingTags = new();
        int lockSkips = 0;
        long totalSaveMs = 0;

        // Unbounded channel: fetch tasks write results as they complete; consumer drains in SaveBatchSize batches.
        var channel = Channel.CreateUnbounded<(CachedClan Clan, ClanFetch Result)>(new UnboundedChannelOptions { SingleReader = true });

        List<Task> allFetchTasks = new();

        try
        {
            // Phase 1: kick off all API calls concurrently.
            foreach (CachedClan cachedClan in cachedClans)
            {
                if (!Synchronizer.ClanLock.TryAcquire(cachedClan.Tag))
                {
                    lockSkips++;
                    continue;
                }

                updatingTags.Add(cachedClan.Tag);

                await Synchronizer.UpdateSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                allFetchTasks.Add(Synchronizer.WithSemaphoreAsync(TryFetchAsync(cachedClan, channel.Writer, cancellationToken)));
            }

            // Complete the channel once every fetch task finishes.
            _ = Task.WhenAll(allFetchTasks).ContinueWith(_ => channel.Writer.Complete(), TaskScheduler.Default);

            // Phase 2: consume completed fetches in SaveBatchSize batches, applying mutations and saving each batch.
            int batchSize = CacheOptions.CurrentValue.SaveBatchSize;
            var noChange = new ClanNoChange();
            var batch = new List<(CachedClan Clan, ClanFetch Result)>(batchSize);

            await foreach (var item in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    ApplyBatch(batch, noChange);
                    var saveSw = System.Diagnostics.Stopwatch.StartNew();
                    await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                    totalSaveMs += saveSw.ElapsedMilliseconds;
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                ApplyBatch(batch, noChange);
                var saveSw = System.Diagnostics.Stopwatch.StartNew();
                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                totalSaveMs += saveSw.ElapsedMilliseconds;
            }

            await BulkUpdateClanKeepUntilAsync(dbContext, noChange, CancellationToken.None).ConfigureAwait(false);

            return new CycleCounters(
                cachedClans.Count,
                updatingTags.Count,
                lockSkips,
                totalSaveMs);
        }
        finally
        {
            foreach (string tag in updatingTags)
                Synchronizer.ClanLock.Release(tag);
        }
    }

    private sealed class ClanNoChange
    {
        public List<(int Id, CachedClan Fetched, DateTime? LastChangedAt)> ClanItems { get; } = new();
        public List<(int Id, CachedClanWarLog Fetched, DateTime? LastChangedAt)> WarLogItems { get; } = new();
        public List<(int Id, CachedClanWarLeagueGroup Fetched, DateTime? LastChangedAt)> GroupItems { get; } = new();
    }

    private async Task BulkUpdateClanKeepUntilAsync(CacheDbContext dbContext, ClanNoChange noChange, CancellationToken ct)
    {
        if (noChange.ClanItems.Count > 0)
        {
            var groups = new Dictionary<TimeSpan, List<int>>();
            foreach (var (id, fetched, lastChangedAt) in noChange.ClanItems)
            {
                TimeSpan ttl = await Ttl.NoChangeTimeToLiveOrDefaultAsync<Rest.Models.Clan>(fetched, lastChangedAt).ConfigureAwait(false);
                if (!groups.TryGetValue(ttl, out List<int>? groupIds))
                    groups[ttl] = groupIds = new();
                groupIds.Add(id);
            }
            foreach (var (ttl, groupIds) in groups)
            {
                DateTime keepUntil = DateTime.UtcNow.Add(ttl);
                foreach (int[] chunk in groupIds.Chunk(100))
                {
                    int[] ids = chunk;
                    await dbContext.Clans
                        .Where(c => ids.Contains(c.Id))
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.KeepUntil, keepUntil), ct)
                        .ConfigureAwait(false);
                }
            }
        }

        if (noChange.WarLogItems.Count > 0)
        {
            var groups = new Dictionary<TimeSpan, List<int>>();
            foreach (var (id, fetched, lastChangedAt) in noChange.WarLogItems)
            {
                TimeSpan ttl = await Ttl.NoChangeTimeToLiveOrDefaultAsync<Rest.Models.ClanWarLog>(fetched, lastChangedAt).ConfigureAwait(false);
                if (!groups.TryGetValue(ttl, out List<int>? groupIds))
                    groups[ttl] = groupIds = new();
                groupIds.Add(id);
            }
            foreach (var (ttl, groupIds) in groups)
            {
                DateTime keepUntil = DateTime.UtcNow.Add(ttl);
                foreach (int[] chunk in groupIds.Chunk(100))
                {
                    int[] ids = chunk;
                    await dbContext.Clans
                        .Where(c => ids.Contains(c.Id))
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.WarLog.KeepUntil, keepUntil), ct)
                        .ConfigureAwait(false);
                }
            }
        }

        if (noChange.GroupItems.Count > 0)
        {
            var groups = new Dictionary<TimeSpan, List<int>>();
            foreach (var (id, fetched, lastChangedAt) in noChange.GroupItems)
            {
                TimeSpan ttl = await Ttl.NoChangeTimeToLiveOrDefaultAsync<Rest.Models.ClanWarLeagueGroup>(fetched, lastChangedAt).ConfigureAwait(false);
                if (!groups.TryGetValue(ttl, out List<int>? groupIds))
                    groups[ttl] = groupIds = new();
                groupIds.Add(id);
            }
            foreach (var (ttl, groupIds) in groups)
            {
                DateTime keepUntil = DateTime.UtcNow.Add(ttl);
                foreach (int[] chunk in groupIds.Chunk(100))
                {
                    int[] ids = chunk;
                    await dbContext.Clans
                        .Where(c => ids.Contains(c.Id))
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.Group.KeepUntil, keepUntil), ct)
                        .ConfigureAwait(false);
                }
            }
        }
    }

    private sealed class ClanFetch
    {
        public CachedClan? Clan { get; set; }
        public CachedClanWarLog? WarLog { get; set; }
        public CachedClanWarLeagueGroup? Group { get; set; }
        public bool ClearGroupAdded { get; set; }
        public DateTime? ExtendedWarKeepUntil { get; set; }
    }

    private static void ApplyBatch(List<(CachedClan Clan, ClanFetch Result)> batch, ClanNoChange noChange)
    {
        foreach (var (cachedClan, result) in batch)
        {
            if (result.ExtendedWarKeepUntil.HasValue)
                cachedClan.CurrentWar.KeepUntil = result.ExtendedWarKeepUntil.Value;

            if (result.Clan != null)
            {
                if (CachedClan.HasUpdated(cachedClan, result.Clan))
                    cachedClan.UpdateFrom(result.Clan);
                else
                    noChange.ClanItems.Add((cachedClan.Id, result.Clan, cachedClan.DownloadedAt));
            }

            if (result.WarLog != null)
            {
                if (CachedClanWarLog.HasUpdated(cachedClan.WarLog, result.WarLog))
                    cachedClan.WarLog.UpdateFrom(result.WarLog);
                else
                    noChange.WarLogItems.Add((cachedClan.Id, result.WarLog, cachedClan.WarLog.DownloadedAt));
            }

            if (result.Group != null)
            {
                if (CachedClanWarLeagueGroup.HasUpdated(cachedClan.Group, result.Group))
                {
                    if (result.ClearGroupAdded)
                        cachedClan.Group.Added = false;
                    cachedClan.Group.UpdateFrom(result.Group);
                }
                else
                    noChange.GroupItems.Add((cachedClan.Id, result.Group, cachedClan.Group.DownloadedAt));
            }
        }
    }

    private async Task TryFetchAsync(CachedClan cachedClan, ChannelWriter<(CachedClan, ClanFetch)> writer, CancellationToken cancellationToken)
    {
        var result = new ClanFetch();
        try
        {
            result.ExtendedWarKeepUntil = GetExtendedWarKeepUntil(cachedClan);

            List<Task> tasks = new();

            IClansApi clansApi = ApiFactory.Create<IClansApi>();

            ClanServiceOptions clanOptions = ClanOptions.CurrentValue;

            Option<bool> realTime = CacheOptions.CurrentValue.RealTime.Contains(Clash.NormalizeTag(cachedClan.Tag)) ? new(true) : default;

            if (clanOptions.DownloadClan && cachedClan.Download && cachedClan.IsExpired)
                tasks.Add(FetchClanAsync(clansApi, cachedClan, result, cancellationToken));

            if (clanOptions.DownloadWarLog && cachedClan.WarLog.Download && cachedClan.WarLog.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                tasks.Add(FetchWarLogAsync(clansApi, cachedClan, result, cancellationToken));

            if (clanOptions.DownloadGroup && cachedClan.Group.Download && cachedClan.Group.IsExpired)
                tasks.Add(FetchGroupAsync(clansApi, realTime, cachedClan, result, cancellationToken));

            await Task.WhenAll(tasks).WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(CacheLogEvents.ClanServiceUpdateFailed, e, "An exception occured while updating clan {tag}", cachedClan.Tag);
        }
        finally
        {
            writer.TryWrite((cachedClan, result));
        }
    }

    private async Task FetchClanAsync(IClansApi clansApi, CachedClan cachedClan, ClanFetch result, CancellationToken cancellationToken)
    {
        try
        {
            CachedClan fetched = await CachedClan.FromClanResponseAsync(cachedClan.Tag, Ttl, clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && ClanUpdated != null && CachedClan.HasUpdated(cachedClan, fetched))
                _fireAndForget.Append(() => ClanUpdated.Invoke(this, new ClanUpdatedEventArgs(cachedClan.Content, fetched.Content, cancellationToken)));

            result.Clan = fetched;
        }
        catch (Exception e)
        {
            _logger.LogError(CacheLogEvents.ClanServiceUpdateFailed, e, "An exception occured while updating clan {tag}", cachedClan.Tag);
            throw;
        }
    }

    private async Task FetchWarLogAsync(IClansApi clansApi, CachedClan cachedClan, ClanFetch result, CancellationToken cancellationToken)
    {
        try
        {
            CachedClanWarLog fetched = await CachedClanWarLog.FromClanWarLogResponseAsync(cachedClan.Tag, Ttl, clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWarLog.HasUpdated(cachedClan.WarLog, fetched) && ClanWarLogUpdated != null)
                _fireAndForget.Append(() => ClanWarLogUpdated.Invoke(this, new ClanWarLogUpdatedEventArgs(cachedClan.WarLog.Content, fetched.Content, cachedClan.Content, cancellationToken)));

            result.WarLog = fetched;
        }
        catch (Exception e)
        {
            _logger.LogError(CacheLogEvents.ClanServiceWarLogUpdateFailed, e, "An exception occured while updating the war log for clan {tag}", cachedClan.Tag);
            throw;
        }
    }

    private async Task FetchGroupAsync(IClansApi clansApi, Option<bool> realtime, CachedClan cachedClan, ClanFetch result, CancellationToken cancellationToken)
    {
        try
        {
            CachedClanWarLeagueGroup fetched = await CachedClanWarLeagueGroup
                .FromClanWarLeagueGroupResponseAsync(cachedClan.Tag, realtime, Ttl, clansApi, cancellationToken)
                .ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWarLeagueGroup.HasUpdated(cachedClan.Group, fetched))
            {
                if (ClanWarLeagueGroupUpdated != null)
                    _fireAndForget.Append(() => ClanWarLeagueGroupUpdated.Invoke(this, new ClanWarLeagueGroupUpdatedEventArgs(cachedClan.Group.Content, fetched.Content, cachedClan.Content, cancellationToken)));

                result.ClearGroupAdded = true;
            }

            result.Group = fetched;
        }
        catch (Exception e)
        {
            _logger.LogError(CacheLogEvents.ClanServiceGroupUpdateFailed, e, "An exception occured while updating the group for clan {tag}", cachedClan.Tag);
            throw;
        }
    }

    private DateTime? GetExtendedWarKeepUntil(CachedClan cachedClan)
    {
        if (CwlWarOptions.CurrentValue.Enabled &&
            (cachedClan.CurrentWar.Content == null || cachedClan.CurrentWar.Content.State == Rest.Models.WarState.WarEnded) &&
            cachedClan.Group.Content?.State != Rest.Models.GroupState.Ended &&
            cachedClan.Group.Content?.Season.Year == DateTime.UtcNow.Year &&
            cachedClan.Group.Content?.Season.Month == DateTime.UtcNow.Month)
            // keep currentwar around an arbitrary amount of time since we are in cwl
            return DateTime.UtcNow.AddMinutes(20);

        return null;
    }
}