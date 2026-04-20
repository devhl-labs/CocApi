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

public sealed class PlayerService : ServiceBase
{
    private readonly ILogger<PlayerService> _logger;

    internal event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;


    internal Synchronizer Synchronizer { get; }
    internal IApiFactory ApiFactory { get; }
    public IOptions<CacheOptions> Options { get; }
    internal static bool Instantiated { get; private set; }
    internal TimeToLiveProvider TimeToLiveProvider { get; }


    public PlayerService(
        ILogger<PlayerService> logger,
        IServiceScopeFactory scopeFactory,
        TimeToLiveProvider timeToLiveProvider,
        Synchronizer synchronizer,
        IApiFactory apiFactory,
        IOptions<CacheOptions> options)
    : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.Players))
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        _logger = logger;
        TimeToLiveProvider = timeToLiveProvider;
        Synchronizer = synchronizer;
        ApiFactory = apiFactory;
        Options = options;
    }


    protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        SetDateVariables();

        PlayerServiceOptions options = Options.Value.Players;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedPlayer> trackedPlayers = await dbContext.Players
            .Where(p =>
                (p.ExpiresAt ?? min) < expires &&
                (p.KeepUntil ?? min) < now &&
                p.Download &&
                p.Id > _id)
            .OrderBy(p => p.Id)
            .Take(options.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _id = trackedPlayers.Count == options.ConcurrentUpdates
            ? trackedPlayers.Max(c => c.Id)
            : int.MinValue;

        HashSet<string> updatingTags = new();
        long totalSaveMs = 0;

        IPlayersApi playersApi = ApiFactory.Create<IPlayersApi>();

        var channel = Channel.CreateUnbounded<(CachedPlayer Player, CachedPlayer? Fetched)>(new UnboundedChannelOptions { SingleReader = true });

        List<Task> allFetchTasks = new();

        try
        {
            foreach (CachedPlayer trackedPlayer in trackedPlayers)
            {
                if (!Synchronizer.VillageLock.TryAcquire(trackedPlayer.Tag))
                    continue;

                updatingTags.Add(trackedPlayer.Tag);

                if (trackedPlayer.Download && trackedPlayer.IsExpired)
                {
                    await Synchronizer.UpdateSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    allFetchTasks.Add(Synchronizer.WithSemaphoreAsync(TryFetchAsync(playersApi, trackedPlayer, channel.Writer, cancellationToken)));
                }
            }

            _ = Task.WhenAll(allFetchTasks).ContinueWith(_ => channel.Writer.Complete(), TaskScheduler.Default);

            int batchSize = Options.Value.SaveBatchSize;
            var batch = new List<(CachedPlayer Player, CachedPlayer? Fetched)>(batchSize);

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
            foreach (string tag in updatingTags)
                Synchronizer.VillageLock.Release(tag);
        }
    }

    private static void ApplyBatch(List<(CachedPlayer Player, CachedPlayer? Fetched)> batch)
    {
        foreach (var (player, fetched) in batch)
            if (fetched != null)
                player.UpdateFrom(fetched);
    }

    private async Task TryFetchAsync(IPlayersApi playersApi, CachedPlayer cachedPlayer, ChannelWriter<(CachedPlayer, CachedPlayer?)> writer, CancellationToken cancellationToken)
    {
        CachedPlayer? fetched = null;
        try
        {
            fetched = await CachedPlayer
                .FromPlayerResponseAsync(cachedPlayer.Tag, TimeToLiveProvider, playersApi, cancellationToken)
                .ConfigureAwait(false);

            if (fetched.Content != null && CachedPlayer.HasUpdated(cachedPlayer, fetched) && PlayerUpdated != null)
                await PlayerUpdated(this, new PlayerUpdatedEventArgs(cachedPlayer.Content, fetched.Content, cancellationToken)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occured while updating player {tag}", cachedPlayer.Tag);
        }
        finally
        {
            writer.TryWrite((cachedPlayer, fetched));
        }
    }
}