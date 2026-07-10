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

public sealed class PlayerService : ServiceBase<PlayerServiceOptions>
{
    private readonly ILogger<PlayerService> _logger;

    internal event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;


    internal Synchronizer Synchronizer { get; }
    internal IApiFactory ApiFactory { get; }
    public IOptions<CacheOptions> CacheOptions { get; }
    internal IOptionsMonitor<PlayerServiceOptions> PlayerOptions { get; }
    internal static bool Instantiated { get; private set; }
    internal TimeToLiveProvider TimeToLiveProvider { get; }


    public PlayerService(
        ILogger<PlayerService> logger,
        IServiceScopeFactory scopeFactory,
        TimeToLiveProvider timeToLiveProvider,
        Synchronizer synchronizer,
        IApiFactory apiFactory,
        IOptions<CacheOptions> cacheOptions,
        IOptionsMonitor<PlayerServiceOptions> playerOptions,
        ILoggerFactory loggerFactory)
    : base(logger, scopeFactory, playerOptions, loggerFactory)
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        _logger = logger;
        TimeToLiveProvider = timeToLiveProvider;
        Synchronizer = synchronizer;
        ApiFactory = apiFactory;
        CacheOptions = cacheOptions;
        PlayerOptions = playerOptions;
    }


    protected override async Task<CycleCounters> ExecuteCycleAsync(CancellationToken cancellationToken)
    {
        PlayerServiceOptions playerOptions = PlayerOptions.CurrentValue;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedPlayer> trackedPlayers = await dbContext.Players
            .Where(p =>
                (p.ExpiresAt ?? min) < expires &&
                (p.KeepUntil ?? min) < now &&
                p.Download &&
                p.Id > _id)
            .OrderBy(p => p.Id)
            .Take(playerOptions.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _id = trackedPlayers.Count == playerOptions.ConcurrentUpdates
            ? trackedPlayers.Max(c => c.Id)
            : int.MinValue;

        HashSet<string> updatingTags = new();
        int lockSkips = 0;
        long totalSaveMs = 0;

        IPlayersApi playersApi = ApiFactory.Create<IPlayersApi>();

        var channel = Channel.CreateUnbounded<(CachedPlayer Player, CachedPlayer? Fetched)>(new UnboundedChannelOptions { SingleReader = true });

        List<Task> allFetchTasks = new();

        try
        {
            foreach (CachedPlayer trackedPlayer in trackedPlayers)
            {
                if (!Synchronizer.VillageLock.TryAcquire(trackedPlayer.Tag))
                {
                    lockSkips++;
                    continue;
                }

                updatingTags.Add(trackedPlayer.Tag);

                if (trackedPlayer.Download && trackedPlayer.IsExpired)
                {
                    await Synchronizer.UpdateSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    allFetchTasks.Add(Synchronizer.WithSemaphoreAsync(TryFetchAsync(playersApi, trackedPlayer, channel.Writer, cancellationToken)));
                }
            }

            _ = Task.WhenAll(allFetchTasks).ContinueWith(_ => channel.Writer.Complete(), TaskScheduler.Default);

            int batchSize = CacheOptions.Value.SaveBatchSize;
            var batch = new List<(CachedPlayer Player, CachedPlayer? Fetched)>(batchSize);

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

            return new CycleCounters(
                trackedPlayers.Count,
                updatingTags.Count,
                lockSkips,
                totalSaveMs);
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