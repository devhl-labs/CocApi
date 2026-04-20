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

public sealed class CwlWarService : ServiceBase
{
    private readonly FireAndForgetService _fireAndForget;

    internal event AsyncEventHandler<WarEventArgs>? ClanWarEndingSoon;
    internal event AsyncEventHandler<WarEventArgs>? ClanWarEnded;
    internal event AsyncEventHandler<WarEventArgs>? ClanWarStartingSoon;
    internal event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

    public ILogger<CwlWarService> Logger { get; }

    internal IApiFactory ApiFactory { get; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider Ttl { get; }
    public IOptions<CacheOptions> Options { get; }
    internal static bool Instantiated { get; private set; }


    public CwlWarService(
        ILogger<CwlWarService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory,
        Synchronizer synchronizer,
        TimeToLiveProvider ttl,
        IOptions<CacheOptions> options,
        FireAndForgetService fireAndForget)
    : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.CwlWars))
    {
        _fireAndForget = fireAndForget;
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        Logger = logger;
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        Ttl = ttl;
        Options = options;
    }


    protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        SetDateVariables();

        CwlWarServiceOptions options = Options.Value.CwlWars;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedWar> cachedWars = await dbContext.Wars
            .Where(w =>
                !string.IsNullOrWhiteSpace(w.WarTag) &&
                (w.ExpiresAt ?? min) < expires &&
                (w.KeepUntil ?? min) < now &&
                !w.IsFinal &&
                w.Id > _id)
            .OrderBy(c => c.Id)
            .Take(options.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _id = cachedWars.Count == options.ConcurrentUpdates
            ? cachedWars.Max(c => c.Id)
            : int.MinValue;

        HashSet<string> updatingCwlWar = new();
        long totalSaveMs = 0;

        var channel = Channel.CreateUnbounded<(CachedWar War, CwlWarFetch Result)>(new UnboundedChannelOptions { SingleReader = true });

        List<Task> allFetchTasks = new();

        try
        {
            IClansApi clansApi = ApiFactory.Create<IClansApi>();

            foreach (CachedWar cachedWar in cachedWars)
            {
                if (Synchronizer.CwlWarLock.TryAcquire(cachedWar.WarTag))
                {
                    updatingCwlWar.Add(cachedWar.WarTag);

                    await Synchronizer.UpdateSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    allFetchTasks.Add(TryFetchAsync(clansApi, cachedWar, Options.Value.RealTime == null ? default : new(Options.Value.RealTime.Value), channel.Writer, cancellationToken));
                }
                else
                {
                    // No lock acquired — still process announcements only (no HTTP).
                    allFetchTasks.Add(TryFetchAnnouncementsOnlyAsync(cachedWar, channel.Writer, cancellationToken));
                }
            }

            _ = Task.WhenAll(allFetchTasks).ContinueWith(_ => channel.Writer.Complete(), TaskScheduler.Default);

            int batchSize = Options.Value.SaveBatchSize;
            var batch = new List<(CachedWar War, CwlWarFetch Result)>(batchSize);

            await foreach (var item in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    ApplyBatch(batch);
                    var saveSw = System.Diagnostics.Stopwatch.StartNew();
                    await Synchronizer.SaveSemaphore.WaitAsync(CancellationToken.None).ConfigureAwait(false);
                    try { await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false); }
                    finally { Synchronizer.SaveSemaphore.Release(); }
                    totalSaveMs += saveSw.ElapsedMilliseconds;
                    dbContext.ChangeTracker.Clear();
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                ApplyBatch(batch);
                var saveSw = System.Diagnostics.Stopwatch.StartNew();
                await Synchronizer.SaveSemaphore.WaitAsync(CancellationToken.None).ConfigureAwait(false);
                try { await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false); }
                finally { Synchronizer.SaveSemaphore.Release(); }
                totalSaveMs += saveSw.ElapsedMilliseconds;
                dbContext.ChangeTracker.Clear();
            }
        }
        finally
        {
            foreach (string tag in updatingCwlWar)
                Synchronizer.CwlWarLock.Release(tag);
        }
    }

    private sealed class CwlWarFetch
    {
        public CachedWar? Source { get; set; }
        public bool IsFinal { get; set; }
        public Announcements NewAnnouncements { get; set; }
    }

    private static void ApplyBatch(List<(CachedWar War, CwlWarFetch Result)> batch)
    {
        foreach (var (cachedWar, result) in batch)
        {
            cachedWar.Announcements |= result.NewAnnouncements;

            if (result.Source != null)
            {
                cachedWar.IsFinal = result.IsFinal;
                cachedWar.UpdateFrom(result.Source);
            }
        }
    }

    private async Task TryFetchAsync(IClansApi clansApi, CachedWar cachedWar, Option<bool> realtime, ChannelWriter<(CachedWar, CwlWarFetch)> writer, CancellationToken cancellationToken)
    {
        var result = new CwlWarFetch();
        try
        {
            try
            {
                await FetchCwlWarAsync(clansApi, cachedWar, realtime, result, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                // Release the semaphore immediately after HTTP — same scope as master's UpdateCwlWarAsync.
                Synchronizer.UpdateSemaphore.Release();
            }

            await GatherAnnouncementsAsync(cachedWar, result, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An exception occured while updating cwl war {warTag}", cachedWar.WarTag);
        }
        finally
        {
            writer.TryWrite((cachedWar, result));
        }
    }

    private async Task TryFetchAnnouncementsOnlyAsync(CachedWar cachedWar, ChannelWriter<(CachedWar, CwlWarFetch)> writer, CancellationToken cancellationToken)
    {
        var result = new CwlWarFetch();
        try
        {
            await GatherAnnouncementsAsync(cachedWar, result, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            writer.TryWrite((cachedWar, result));
        }
    }

    private async Task FetchCwlWarAsync(IClansApi clansApi, CachedWar cachedWar, Option<bool> realtime, CwlWarFetch result, CancellationToken cancellationToken)
    {
        CachedWar fetched = await CachedWar
                .FromClanWarLeagueWarResponseAsync(cachedWar.WarTag, cachedWar.Season.Value, realtime, Ttl, clansApi, cancellationToken)
                .ConfigureAwait(false);

        if (cachedWar.Content != null &&
            fetched.Content != null &&
            cachedWar.Season == fetched.Season &&
            CachedWar.HasUpdated(cachedWar, fetched) &&
            ClanWarUpdated != null)
        {
            _fireAndForget.Append(() => ClanWarUpdated.Invoke(this, new ClanWarUpdatedEventArgs(cachedWar.Content, fetched.Content, null, null, cancellationToken)));
        }

        result.IsFinal = (fetched.Content == null && !Clash.IsCwlEnabled) || fetched.State == Rest.Models.WarState.WarEnded;
        result.Source = fetched;
    }

    private Task GatherAnnouncementsAsync(CachedWar cachedWar, CwlWarFetch result, CancellationToken cancellationToken)
    {
        if (cachedWar.Content == null)
            return Task.CompletedTask;

        if (cachedWar.Announcements.HasFlag(Announcements.WarStartingSoon) == false &&
            now > cachedWar.Content.StartTime.AddHours(-1) &&
            now < cachedWar.Content.StartTime)
        {
            result.NewAnnouncements |= Announcements.WarStartingSoon;

            if (ClanWarStartingSoon != null)
                _fireAndForget.Append(() => ClanWarStartingSoon.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)));
        }

        if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
            now > cachedWar.Content.EndTime.AddHours(-1) &&
            now < cachedWar.Content.EndTime)
        {
            result.NewAnnouncements |= Announcements.WarEndingSoon;

            if (ClanWarEndingSoon != null)
                _fireAndForget.Append(() => ClanWarEndingSoon.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)));
        }

        if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
            cachedWar.EndTime < now &&
            cachedWar.EndTime.Day <= (now.Day + 1))
        {
            result.NewAnnouncements |= Announcements.WarEnded;

            if (ClanWarEnded != null)
                _fireAndForget.Append(() => ClanWarEnded.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)));
        }

        return Task.CompletedTask;
    }
}