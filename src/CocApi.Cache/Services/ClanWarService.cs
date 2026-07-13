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

public sealed class ClanWarService : ServiceBase<ClanWarServiceOptions>
{
    private readonly ILogger<ClanWarService> _logger;


    internal IApiFactory ApiFactory { get; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider Ttl { get; }
    internal IOptionsMonitor<CacheOptions> CacheOptions { get; }
    internal IOptionsMonitor<ClanWarServiceOptions> ClanWarOptions { get; }
    internal IOptionsMonitor<CwlWarServiceOptions> CwlWarOptions { get; }
    internal static bool Instantiated { get; private set; }

    public ClanWarService(
        ILogger<ClanWarService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory,
        Synchronizer synchronizer,
        TimeToLiveProvider ttl,
        IOptionsMonitor<CacheOptions> cacheOptions,
        IOptionsMonitor<ClanWarServiceOptions> clanWarOptions,
        IOptionsMonitor<CwlWarServiceOptions> cwlWarOptions,
        ILoggerFactory loggerFactory)
        : base(logger, scopeFactory, clanWarOptions, loggerFactory)
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        Ttl = ttl;
        CacheOptions = cacheOptions;
        ClanWarOptions = clanWarOptions;
        CwlWarOptions = cwlWarOptions;
        _logger = logger;
    }


    protected override async Task<CycleCounters> ExecuteCycleAsync(CancellationToken cancellationToken)
    {
        ClanWarServiceOptions clanWarOptions = ClanWarOptions.CurrentValue;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedClan> cachedClans = await dbContext.Clans
            .Where(c =>
                c.Id > _id &&
                    clanWarOptions.DownloadCurrentWar && (c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < now && c.CurrentWar.Download && c.IsWarLogPublic != false
                )
            .OrderBy(c => c.Id)
            .Take(clanWarOptions.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _id = cachedClans.Count == clanWarOptions.ConcurrentUpdates
            ? cachedClans.Max(c => c.Id)
            : int.MinValue;

        HashSet<string> updatingTags = new();
        int lockSkips = 0;
        long totalSaveMs = 0;

        var channel = Channel.CreateUnbounded<(CachedClan Clan, ClanWarFetch Result)>(new UnboundedChannelOptions { SingleReader = true });

        List<Task> allFetchTasks = new();

        try
        {
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

            _ = Task.WhenAll(allFetchTasks).ContinueWith(_ => channel.Writer.Complete(), TaskScheduler.Default);

            int batchSize = CacheOptions.CurrentValue.SaveBatchSize;
            var batch = new List<(CachedClan Clan, ClanWarFetch Result)>(batchSize);
            var noChangeItems = new List<(int Id, CachedClanWar Fetched, DateTime? LastChangedAt)>();

            await foreach (var item in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    ApplyBatch(batch, noChangeItems);
                    var saveSw = System.Diagnostics.Stopwatch.StartNew();
                    await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                    totalSaveMs += saveSw.ElapsedMilliseconds;
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                ApplyBatch(batch, noChangeItems);
                var saveSw = System.Diagnostics.Stopwatch.StartNew();
                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                totalSaveMs += saveSw.ElapsedMilliseconds;
            }

            await BulkUpdateCurrentWarKeepUntilAsync(dbContext, noChangeItems, CancellationToken.None).ConfigureAwait(false);

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

    private sealed class ClanWarFetch
    {
        public CachedClanWar? CurrentWar { get; set; }
        public bool IsNewWar { get; set; }
        public Rest.Models.WarType? NewWarType { get; set; }
        public DateTime? ExtendedWarKeepUntil { get; set; }
    }

    private static void ApplyBatch(List<(CachedClan Clan, ClanWarFetch Result)> batch, List<(int Id, CachedClanWar Fetched, DateTime? LastChangedAt)> noChangeItems)
    {
        foreach (var (cachedClan, result) in batch)
        {
            if (result.ExtendedWarKeepUntil.HasValue)
                cachedClan.CurrentWar.KeepUntil = result.ExtendedWarKeepUntil.Value;

            if (result.CurrentWar != null)
            {
                if (result.IsNewWar)
                {
                    cachedClan.CurrentWar.Type = result.NewWarType;
                    cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
                }

                if (result.CurrentWar.Content == null || result.IsNewWar || CachedClanWar.HasUpdated(cachedClan.CurrentWar, result.CurrentWar))
                    cachedClan.CurrentWar.UpdateFrom(result.CurrentWar);
                else if (!result.ExtendedWarKeepUntil.HasValue)
                    noChangeItems.Add((cachedClan.Id, result.CurrentWar, cachedClan.CurrentWar.DownloadedAt));
            }
        }
    }

    private async Task BulkUpdateCurrentWarKeepUntilAsync(CacheDbContext dbContext, List<(int Id, CachedClanWar Fetched, DateTime? LastChangedAt)> noChangeItems, CancellationToken ct)
    {
        if (noChangeItems.Count == 0) return;
        var groups = new Dictionary<TimeSpan, List<int>>();
        foreach (var (id, fetched, lastChangedAt) in noChangeItems)
        {
            TimeSpan ttl = await Ttl.NoChangeTimeToLiveOrDefaultAsync<Rest.Models.ClanWar>(fetched, lastChangedAt).ConfigureAwait(false);
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
                    .ExecuteUpdateAsync(s => s.SetProperty(c => c.CurrentWar.KeepUntil, keepUntil), ct)
                    .ConfigureAwait(false);
            }
        }
    }

    private async Task TryFetchAsync(CachedClan cachedClan, ChannelWriter<(CachedClan, ClanWarFetch)> writer, CancellationToken cancellationToken)
    {
        var result = new ClanWarFetch();
        try
        {
            result.ExtendedWarKeepUntil = GetExtendedWarKeepUntil(cachedClan);

            IClansApi clansApi = ApiFactory.Create<IClansApi>();

            ClanWarServiceOptions clanWarOptions = ClanWarOptions.CurrentValue;

            Option<bool> realTime = CacheOptions.CurrentValue.RealTime.Contains(Clash.NormalizeTag(cachedClan.Tag)) ? new(true) : default;

            if (clanWarOptions.DownloadCurrentWar && cachedClan.CurrentWar.Download && cachedClan.CurrentWar.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                await FetchClanWarAsync(clansApi, cachedClan, result, realTime, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(CacheLogEvents.ClanWarServiceUpdateFailed, e, "An exception occured while updating clan {tag}", cachedClan.Tag);
        }
        finally
        {
            writer.TryWrite((cachedClan, result));
        }
    }

    private async Task FetchClanWarAsync(IClansApi clansApi, CachedClan cachedClan, ClanWarFetch result, Option<bool> realtime, CancellationToken cancellationToken)
    {
        try
        {
            CachedClanWar fetched = await CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, realtime, Ttl, clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                result.IsNewWar = true;
                result.NewWarType = fetched.Content.GetWarType();
            }

            result.CurrentWar = fetched;
        }
        catch (Exception e)
        {
            _logger.LogError(CacheLogEvents.ClanWarServiceFetchFailed, e, "An exception occured while updating the war for clan {tag}", cachedClan.Tag);
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