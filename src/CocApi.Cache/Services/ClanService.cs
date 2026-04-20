using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CocApi.Rest.Apis;
using CocApi.Cache.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CocApi.Rest.Client;
using CocApi.Cache.Services.Options;

namespace CocApi.Cache.Services;

public sealed class ClanService : ServiceBase
{

    private readonly ILogger<ClanService> _logger;
    private readonly FireAndForgetService _fireAndForget;


    internal event AsyncEventHandler<ClanUpdatedEventArgs>? ClanUpdated;
    internal event AsyncEventHandler<ClanWarLeagueGroupUpdatedEventArgs>? ClanWarLeagueGroupUpdated;
    internal event AsyncEventHandler<ClanWarLogUpdatedEventArgs>? ClanWarLogUpdated;


    internal IApiFactory ApiFactory { get; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider Ttl { get; }
    internal IOptions<CacheOptions> Options { get; }
    internal static bool Instantiated { get; private set; }

    public ClanService(
        ILogger<ClanService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory,
        Synchronizer synchronizer,
        TimeToLiveProvider ttl,
        IOptions<CacheOptions> options,
        FireAndForgetService fireAndForget)
        : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.Clans))
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        Ttl = ttl;
        Options = options;
        _logger = logger;
        _fireAndForget = fireAndForget;
    }


    protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        var cycleSw = System.Diagnostics.Stopwatch.StartNew();
        SetDateVariables();

        ClanServiceOptions options = Options.Value.Clans;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedClan> cachedClans = await dbContext.Clans
            .Where(c =>
                c.Id > _id &&
                (
                    (options.DownloadClan && (c.ExpiresAt ?? min) < expires && (c.KeepUntil ?? min) < now && c.Download) ||
                    (options.DownloadGroup && (c.Group.ExpiresAt ?? min) < expires && (c.Group.KeepUntil ?? min) < now && c.Group.Download) ||
                    // (options.DownloadCurrentWar && (c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < now && c.CurrentWar.Download && c.IsWarLogPublic != false) ||
                    (options.DownloadWarLog && (c.WarLog.ExpiresAt ?? min) < expires && (c.WarLog.KeepUntil ?? min) < now && c.WarLog.Download && c.IsWarLogPublic != false)
                ))
            .OrderBy(c => c.Id)
            .Take(options.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _id = cachedClans.Count == options.ConcurrentUpdates
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
            int batchSize = Options.Value.SaveBatchSize;
            var batch = new List<(CachedClan Clan, ClanFetch Result)>(batchSize);

            await foreach (var item in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    ApplyBatch(batch);
                    var saveSw = System.Diagnostics.Stopwatch.StartNew();
                    await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                    totalSaveMs += saveSw.ElapsedMilliseconds;
                    dbContext.ChangeTracker.Clear();
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                ApplyBatch(batch);
                var saveSw = System.Diagnostics.Stopwatch.StartNew();
                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                totalSaveMs += saveSw.ElapsedMilliseconds;
                dbContext.ChangeTracker.Clear();
            }

            cycleSw.Stop();
            _logger.LogDebug("ClanService cycle | Fetched={Fetched} | Updated={Updated} | LockSkips={LockSkips} | SaveMs={SaveMs} | TotalMs={TotalMs}",
                cachedClans.Count, updatingTags.Count, lockSkips, totalSaveMs, cycleSw.ElapsedMilliseconds);
            if (cycleSw.ElapsedMilliseconds > 5000)
                _logger.LogWarning("ClanService cycle slow | Fetched={Fetched} | Updated={Updated} | LockSkips={LockSkips} | SaveMs={SaveMs} | TotalMs={TotalMs}",
                    cachedClans.Count, updatingTags.Count, lockSkips, totalSaveMs, cycleSw.ElapsedMilliseconds);
        }
        finally
        {
            foreach (string tag in updatingTags)
                Synchronizer.ClanLock.Release(tag);
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

    private static void ApplyBatch(List<(CachedClan Clan, ClanFetch Result)> batch)
    {
        foreach (var (cachedClan, result) in batch)
        {
            if (result.ExtendedWarKeepUntil.HasValue)
                cachedClan.CurrentWar.KeepUntil = result.ExtendedWarKeepUntil.Value;

            if (result.Clan != null)
                cachedClan.UpdateFrom(result.Clan);

            if (result.WarLog != null)
                cachedClan.WarLog.UpdateFrom(result.WarLog);

            if (result.Group != null)
            {
                if (result.ClearGroupAdded)
                    cachedClan.Group.Added = false;
                cachedClan.Group.UpdateFrom(result.Group);
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

            ClanServiceOptions options = Options.Value.Clans;

            Option<bool> realTime = Options.Value.RealTime == null ? default : new(Options.Value.RealTime.Value);

            if (options.DownloadClan && cachedClan.Download && cachedClan.IsExpired)
                tasks.Add(FetchClanAsync(clansApi, cachedClan, result, cancellationToken));

            if (options.DownloadWarLog && cachedClan.WarLog.Download && cachedClan.WarLog.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                tasks.Add(FetchWarLogAsync(clansApi, cachedClan, result, cancellationToken));

            if (options.DownloadGroup && cachedClan.Group.Download && cachedClan.Group.IsExpired)
                tasks.Add(FetchGroupAsync(clansApi, realTime, cachedClan, result, cancellationToken));

            await Task.WhenAll(tasks).WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occured while updating clan {tag}", cachedClan.Tag);
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
            _logger.LogError(e, "An exception occured while updating clan {tag}", cachedClan.Tag);
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
            _logger.LogError(e, "An exception occured while updating the war log for clan {tag}", cachedClan.Tag);
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
            _logger.LogError(e, "An exception occured while updating the group for clan {tag}", cachedClan.Tag);
            throw;
        }
    }

    private DateTime? GetExtendedWarKeepUntil(CachedClan cachedClan)
    {
        if (!Clash.IsCwlEnabled ||
            !Options.Value.CwlWars.Enabled ||
            cachedClan.CurrentWar.Content?.State == Rest.Models.WarState.InWar ||
            cachedClan.CurrentWar.Content?.State == Rest.Models.WarState.Preparation ||
            cachedClan.Group.Content == null ||
            cachedClan.Group.Content.State == Rest.Models.GroupState.Ended ||
            cachedClan.Group.Content.Season.Month < DateTime.UtcNow.Month ||
            cachedClan.Group.Content.Season.Year < DateTime.UtcNow.Year ||
            (cachedClan.Group.KeepUntil.HasValue && cachedClan.Group.KeepUntil.Value.Month > DateTime.UtcNow.Month) ||
            cachedClan.Group.StatusCode != System.Net.HttpStatusCode.OK)
            return null;

        // keep currentwar around an arbitrary amount of time since we are in cwl
        return DateTime.UtcNow.AddMinutes(20);
    }
}