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

public sealed class ClanWarService : ServiceBase
{
    private readonly ILogger<ClanWarService> _logger;


    internal IApiFactory ApiFactory { get; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider Ttl { get; }
    internal IOptions<CacheOptions> Options { get; }
    internal static bool Instantiated { get; private set; }

    public ClanWarService(
        ILogger<ClanWarService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory,
        Synchronizer synchronizer,
        TimeToLiveProvider ttl,
        IOptions<CacheOptions> options)
        : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.Clans))
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        Ttl = ttl;
        Options = options;
        _logger = logger;
    }


    protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        var cycleSw = System.Diagnostics.Stopwatch.StartNew();
        SetDateVariables();

        ClanWarServiceOptions options = Options.Value.ClanWars;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedClan> cachedClans = await dbContext.Clans
            .Where(c =>
                c.Id > _id &&
                    options.DownloadCurrentWar && (c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < now && c.CurrentWar.Download && c.IsWarLogPublic != false
                )
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

            int batchSize = Options.Value.SaveBatchSize;
            var batch = new List<(CachedClan Clan, ClanWarFetch Result)>(batchSize);

            await foreach (var item in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    ApplyBatch(batch);
                    var saveSw = System.Diagnostics.Stopwatch.StartNew();
                    await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                    totalSaveMs += saveSw.ElapsedMilliseconds;
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                ApplyBatch(batch);
                var saveSw = System.Diagnostics.Stopwatch.StartNew();
                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                totalSaveMs += saveSw.ElapsedMilliseconds;
            }

            cycleSw.Stop();
            _logger.LogDebug("ClanWarService cycle | Fetched={Fetched} | Updated={Updated} | LockSkips={LockSkips} | SaveMs={SaveMs} | TotalMs={TotalMs}",
                cachedClans.Count, updatingTags.Count, lockSkips, totalSaveMs, cycleSw.ElapsedMilliseconds);
            if (cycleSw.ElapsedMilliseconds > 5000)
                _logger.LogWarning("ClanWarService cycle slow | Fetched={Fetched} | Updated={Updated} | LockSkips={LockSkips} | SaveMs={SaveMs} | TotalMs={TotalMs}",
                    cachedClans.Count, updatingTags.Count, lockSkips, totalSaveMs, cycleSw.ElapsedMilliseconds);
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

    private static void ApplyBatch(List<(CachedClan Clan, ClanWarFetch Result)> batch)
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
                    cachedClan.CurrentWar.UpdateFrom(result.CurrentWar);
                }
                else if (CachedClanWar.HasUpdated(cachedClan.CurrentWar, result.CurrentWar))
                    cachedClan.CurrentWar.UpdateFrom(result.CurrentWar);
                else if ((result.CurrentWar.Content?.State == Rest.Models.WarState.NotInWar) || (result.CurrentWar.Content?.State == null))
                {
                    var activity = CachedClan.GetActivityLevel(cachedClan);
                    if (activity != CachedClan.ClanActivityLevel.Active)
                    {
                        var cap = activity == CachedClan.ClanActivityLevel.Dead ? TimeSpan.FromHours(24) : TimeSpan.FromHours(4);
                        cachedClan.CurrentWar.Backoff(cap);
                    }
                }
                // InWar/Preparation, or Active clan: no KeepUntil change
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

            ClanWarServiceOptions options = Options.Value.ClanWars;

            Option<bool> realTime = Options.Value.RealTime == null ? default : new(Options.Value.RealTime.Value);

            if (options.DownloadCurrentWar && cachedClan.CurrentWar.Download && cachedClan.CurrentWar.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                await FetchClanWarAsync(clansApi, cachedClan, result, realTime, cancellationToken).ConfigureAwait(false);
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
            _logger.LogError(e, "An exception occured while updating the war for clan {tag}", cachedClan.Tag);
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