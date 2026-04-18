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

public sealed class WarService : ServiceBase
{
    internal event AsyncEventHandler<WarEventArgs>? ClanWarEndingSoon;
    internal event AsyncEventHandler<WarEventArgs>? ClanWarEndNotSeen;
    internal event AsyncEventHandler<WarEventArgs>? ClanWarEnded;
    internal event AsyncEventHandler<WarEventArgs>? ClanWarStartingSoon;
    internal event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

    public ILogger<WarService> Logger { get; }

    internal IApiFactory ApiFactory { get; }
    internal static bool Instantiated { get; private set; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider TimeToLiveProvider { get; }
    public IOptions<CacheOptions> Options { get; }

    private readonly SemaphoreSlim _semaphore = new(1, 1);


    public WarService(
        ILogger<WarService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory,
        Synchronizer synchronizer,
        TimeToLiveProvider timeToLiveProvider,
        IOptions<CacheOptions> options)
    : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.Wars))
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        Logger = logger;
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        TimeToLiveProvider = timeToLiveProvider;
        Options = options;
    }

    protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        var cycleSw = System.Diagnostics.Stopwatch.StartNew();
        SetDateVariables();

        WarServiceOptions options = Options.Value.Wars;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedWar> cachedWars = await dbContext.Wars
            .Where(w =>
                string.IsNullOrWhiteSpace(w.WarTag) &&
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

        HashSet<string> tags = new();

        foreach (CachedWar cachedWar in cachedWars)
        {
            tags.Add(cachedWar.ClanTag);
            tags.Add(cachedWar.OpponentTag);
        }

        List<CachedClan> allCachedClans = await dbContext.Clans
            .Where(c => tags.Contains(c.Tag))
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        HashSet<string> acquiredWars = new();
        int lockSkips = 0;
        long totalSaveMs = 0;

        IClansApi clansApi = ApiFactory.Create<IClansApi>();

        var channel = Channel.CreateUnbounded<(CachedWar War, WarFetch Result)>(new UnboundedChannelOptions { SingleReader = true });

        List<Task> allFetchTasks = new();

        try
        {
            foreach (CachedWar cachedWar in cachedWars)
            {
                if (!Synchronizer.WarLock.TryAcquire(cachedWar.Key))
                {
                    lockSkips++;
                    continue;
                }

                acquiredWars.Add(cachedWar.Key);

                List<CachedClan> cachedClans = allCachedClans.Where(c => c.Tag == cachedWar.ClanTag || c.Tag == cachedWar.OpponentTag).ToList();

                await Synchronizer.UpdateSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                allFetchTasks.Add(Synchronizer.WithSemaphoreAsync(
                    TryFetchAsync(clansApi, dbContext, cachedWar,
                        cachedClans.FirstOrDefault(c => c.Tag == cachedWar.ClanTag),
                        cachedClans.FirstOrDefault(c => c.Tag == cachedWar.OpponentTag),
                        cachedClans.Select(c => c.CurrentWar).ToArray(),
                        channel.Writer,
                        cancellationToken)));
            }

            _ = Task.WhenAll(allFetchTasks).ContinueWith(_ => channel.Writer.Complete(), TaskScheduler.Default);

            int batchSize = Options.Value.SaveBatchSize;
            var batch = new List<(CachedWar War, WarFetch Result)>(batchSize);

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
            Logger.LogDebug("WarService cycle | Fetched={Fetched} | Updated={Updated} | LockSkips={LockSkips} | SaveMs={SaveMs} | TotalMs={TotalMs}",
                cachedWars.Count, acquiredWars.Count, lockSkips, totalSaveMs, cycleSw.ElapsedMilliseconds);
            if (cycleSw.ElapsedMilliseconds > 5000)
                Logger.LogWarning("WarService cycle slow | Fetched={Fetched} | Updated={Updated} | LockSkips={LockSkips} | SaveMs={SaveMs} | TotalMs={TotalMs}",
                    cachedWars.Count, acquiredWars.Count, lockSkips, totalSaveMs, cycleSw.ElapsedMilliseconds);
        }
        finally
        {
            foreach (string key in acquiredWars)
                Synchronizer.WarLock.Release(key);
        }
    }

    private sealed class WarFetch
    {
        public CachedClanWar? Source { get; set; }
        public bool IsFinal { get; set; }
        public bool SetFinal { get; set; }
        public DateTime? KeepUntil { get; set; }
        public Announcements NewAnnouncements { get; set; }
    }

    private static void ApplyBatch(List<(CachedWar War, WarFetch Result)> batch)
    {
        foreach (var (cachedWar, result) in batch)
        {
            cachedWar.Announcements |= result.NewAnnouncements;

            if (result.KeepUntil.HasValue)
                cachedWar.KeepUntil = result.KeepUntil.Value;

            if (result.SetFinal)
                cachedWar.IsFinal = true;
            else if (result.Source != null)
            {
                cachedWar.UpdateFrom(result.Source);
                cachedWar.IsFinal = result.IsFinal;
            }
        }
    }

    private async Task TryFetchAsync(
        IClansApi clansApi,
        CacheDbContext dbContext,
        CachedWar cachedWar,
        CachedClan? cachedClan,
        CachedClan? cachedOpponent,
        CachedClanWar[] cachedClanWars,
        ChannelWriter<(CachedWar, WarFetch)> writer,
        CancellationToken cancellationToken)
    {
        var result = new WarFetch();
        try
        {
            await Task.WhenAll(
                ComputeWarAsync(clansApi, dbContext, cachedWar, cachedClan, cachedOpponent, result, cancellationToken),
                GatherAnnouncementsAsync(cachedWar, cachedClanWars, result, cancellationToken))
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An exception occured while updating war {id}", cachedWar.Id);
        }
        finally
        {
            writer.TryWrite((cachedWar, result));
        }
    }

    private async Task ComputeWarAsync(
        IClansApi clansApi,
        CacheDbContext dbContext,
        CachedWar cachedWar,
        CachedClan? cachedClan,
        CachedClan? cachedOpponent,
        WarFetch result,
        CancellationToken cancellationToken)
    {
        try
        {
            if (cachedClan == null && cachedOpponent?.CurrentWar.IsExpired == true)
                cachedClan = await CreateCachedClan(clansApi, cachedWar.ClanTag, dbContext, cancellationToken).ConfigureAwait(false);

            if (cachedOpponent == null && cachedClan?.CurrentWar.IsExpired == true)
                cachedOpponent = await CreateCachedClan(clansApi, cachedWar.OpponentTag, dbContext, cancellationToken).ConfigureAwait(false);

            List<CachedClan?> cachedClans = new() { cachedClan, cachedOpponent };

            if (cachedClans.All(c => c?.CurrentWar.PreparationStartTime > cachedWar.PreparationStartTime) || cachedWar.EndTime.AddDays(8) < now)
            {
                result.SetFinal = true;
                return;
            }

            CachedClan? clan = cachedClans
                .OrderByDescending(c => c?.CurrentWar.ExpiresAt)
                .FirstOrDefault(c => c?.CurrentWar.PreparationStartTime == cachedWar.PreparationStartTime);

            if (cachedWar.Content != null &&
                clan?.CurrentWar.Content != null &&
                CachedWar.HasUpdated(cachedWar, clan.CurrentWar) &&
                ClanWarUpdated != null)
                await ClanWarUpdated.Invoke(this, new ClanWarUpdatedEventArgs(
                        cachedWar.Content,
                        clan.CurrentWar.Content,
                        cachedClan?.Content,
                        cachedOpponent?.Content,
                        cancellationToken))
                    .ConfigureAwait(false);

            if (clan != null)
            {
                result.Source = clan.CurrentWar;
                result.IsFinal = clan.CurrentWar.State == Rest.Models.WarState.WarEnded;
            }
            else if (cachedClans.All(c =>
                    c != null &&
                    (
                        c.CurrentWar.PreparationStartTime == DateTime.MinValue ||
                        c.CurrentWar.PreparationStartTime > cachedWar.PreparationStartTime
                    )))
                result.SetFinal = true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to update war clanTag: {cachedWar}", cachedWar.Id);
            result.KeepUntil = DateTime.UtcNow.AddHours(1);
            throw;
        }
    }

    private async Task<CachedClan?> CreateCachedClan(IClansApi clansApi, string tag, CacheDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!Synchronizer.ClanLock.TryAcquire(tag))
            return null;

        try
        {
            CachedClanWar cachedClanWar = await CachedClanWar
                .FromCurrentWarResponseAsync(tag, Options.Value.RealTime == null ? default : new(Options.Value.RealTime.Value), TimeToLiveProvider, clansApi, cancellationToken)
                .ConfigureAwait(false);

            cachedClanWar.Added = true;

            cachedClanWar.Download = false;

            CachedClan cachedClan = new(tag, false, false, false, false, false)
            {
                CurrentWar = cachedClanWar
            };

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                dbContext.Clans.Add(cachedClan);
            }
            finally
            {
                _semaphore.Release();
            }

            return cachedClan;
        }
        finally
        {
            Synchronizer.ClanLock.Release(tag);
        }
    }

    private async Task GatherAnnouncementsAsync(CachedWar cachedWar, CachedClanWar[] cachedClanWars, WarFetch result, CancellationToken cancellationToken)
    {
        try
        {
            if (cachedWar.Content == null)
                return;

            if (cachedWar.Announcements.HasFlag(Announcements.WarStartingSoon) == false &&
                now > cachedWar.Content.StartTime.AddHours(-1) &&
                now < cachedWar.Content.StartTime)
            {
                result.NewAnnouncements |= Announcements.WarStartingSoon;

                if (ClanWarStartingSoon != null)
                    await ClanWarStartingSoon.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
                now > cachedWar.Content.EndTime.AddHours(-1) &&
                now < cachedWar.Content.EndTime)
            {
                result.NewAnnouncements |= Announcements.WarEndingSoon;

                if (ClanWarEndingSoon != null)
                    await ClanWarEndingSoon.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEndNotSeen) == false &&
                cachedWar.State != Rest.Models.WarState.WarEnded &&
                now > cachedWar.EndTime &&
                now < cachedWar.EndTime.AddHours(1) &&
                cachedWar.Content.AllAttacksAreUsed() == false &&
                cachedClanWars != null &&
                cachedClanWars.All(w => w.Content != null && w.Content.PreparationStartTime != cachedWar.Content.PreparationStartTime))
            {
                result.NewAnnouncements |= Announcements.WarEndNotSeen;

                if (ClanWarEndNotSeen != null)
                    await ClanWarEndNotSeen.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
                cachedWar.EndTime < now &&
                cachedWar.EndTime.Day <= (now.Day + 1))
            {
                result.NewAnnouncements |= Announcements.WarEnded;

                if (ClanWarEnded != null)
                    await ClanWarEnded.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            if (!cancellationToken.IsCancellationRequested)
                Logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", GetType().Name, nameof(GatherAnnouncementsAsync));
        }
    }
}