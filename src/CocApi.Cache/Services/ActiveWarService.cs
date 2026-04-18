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

public sealed class ActiveWarService : ServiceBase
{
    private readonly ILogger<ActiveWarService> _logger;

    internal IApiFactory ApiFactory { get; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider Ttl { get; }
    internal IOptions<CacheOptions> Options { get; }
    internal static bool Instantiated { get; private set; }


    public ActiveWarService(
        ILogger<ActiveWarService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory,
        Synchronizer synchronizer,
        TimeToLiveProvider ttl,
        IOptions<CacheOptions> options)
        : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.ActiveWars))
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        _logger = logger;
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        Ttl = ttl;
        Options = options;
    }


    protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        SetDateVariables();

        ActiveWarServiceOptions options = Options.Value.ActiveWars;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedClan> cachedClans = await
            (
                from c in dbContext.Clans
                join w in dbContext.Wars on c.Tag equals w.ClanTag
                where
                    !c.CurrentWar.Download &&
                    (c.Download == false || c.Download && c.IsWarLogPublic == true) &&
                    (c.CurrentWar.ExpiresAt ?? min) < expires &&
                    (c.CurrentWar.KeepUntil ?? min) < now &&
                    c.Id > _id &&
                    !w.IsFinal
                orderby c.Id
                select c
            ).Union(
                from c in dbContext.Clans
                join w in dbContext.Wars on c.Tag equals w.OpponentTag
                where
                    !c.CurrentWar.Download &&
                    (c.Download == false || c.Download && c.IsWarLogPublic == true) &&
                    (c.CurrentWar.ExpiresAt ?? min) < expires &&
                    (c.CurrentWar.KeepUntil ?? min) < now &&
                    c.Id > _id &&
                    !w.IsFinal
                orderby c.Id
                select c
            )
            .Distinct()
            .OrderBy(w => w.Id)
            .Take(options.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _id = cachedClans.Count == options.ConcurrentUpdates
            ? cachedClans.Max(c => c.Id)
            : int.MinValue;

        HashSet<string> updatingTags = new();
        long totalSaveMs = 0;

        var channel = Channel.CreateUnbounded<(CachedClan Clan, ActiveWarFetch Result)>(new UnboundedChannelOptions { SingleReader = true });

        List<Task> allFetchTasks = new();

        try
        {
            foreach (CachedClan cachedClan in cachedClans)
            {
                if (!Synchronizer.ClanLock.TryAcquire(cachedClan.Tag))
                    continue;

                updatingTags.Add(cachedClan.Tag);

                await Synchronizer.UpdateSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                allFetchTasks.Add(Synchronizer.WithSemaphoreAsync(TryFetchAsync(cachedClan, Options.Value.RealTime == null ? default : new(Options.Value.RealTime.Value), channel.Writer, cancellationToken)));
            }

            _ = Task.WhenAll(allFetchTasks).ContinueWith(_ => channel.Writer.Complete(), TaskScheduler.Default);

            int batchSize = Options.Value.SaveBatchSize;
            var batch = new List<(CachedClan Clan, ActiveWarFetch Result)>(batchSize);

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
        }
        finally
        {
            foreach (string tag in updatingTags)
                Synchronizer.ClanLock.Release(tag);
        }
    }

    private sealed class ActiveWarFetch
    {
        public CachedClanWar? CurrentWar { get; set; }
        public bool IsNewWar { get; set; }
        public Rest.Models.WarType? NewWarType { get; set; }
    }

    private static void ApplyBatch(List<(CachedClan Clan, ActiveWarFetch Result)> batch)
    {
        foreach (var (cachedClan, result) in batch)
        {
            if (result.CurrentWar != null)
            {
                if (result.IsNewWar)
                {
                    cachedClan.CurrentWar.Type = result.NewWarType;
                    cachedClan.CurrentWar.Added = false;
                }

                cachedClan.CurrentWar.UpdateFrom(result.CurrentWar);
            }
        }
    }

    private async Task TryFetchAsync(CachedClan cachedClan, Option<bool> realtime, ChannelWriter<(CachedClan, ActiveWarFetch)> writer, CancellationToken cancellationToken)
    {
        var result = new ActiveWarFetch();
        try
        {
            IClansApi clansApi = ApiFactory.Create<IClansApi>();

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
            _logger.LogError(e, "An exception occured while updating clan {tag}", cachedClan.Tag);
        }
        finally
        {
            writer.TryWrite((cachedClan, result));
        }
    }
}
