using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache.Context;
using CocApi.Cache.Services.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Cache.Services;

public sealed class NewWarService : ServiceBase<NewWarServiceOptions>
{
    internal event AsyncEventHandler<WarAddedEventArgs>? ClanWarAdded;


    internal Synchronizer Synchronizer { get; }
    internal IOptionsMonitor<NewWarServiceOptions> NewWarOptions { get; }
    internal static bool Instantiated { get; private set; }


    public NewWarService(
        ILogger<NewWarService> logger,
        IServiceScopeFactory scopeFactory,
        Synchronizer synchronizer,
        IOptionsMonitor<NewWarServiceOptions> newWarOptions,
        ILoggerFactory loggerFactory)
    : base(logger, scopeFactory, newWarOptions, loggerFactory)
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        Synchronizer = synchronizer;
        NewWarOptions = newWarOptions;
    }


    protected override async Task<CycleCounters> ExecuteCycleAsync(CancellationToken cancellationToken)
    {
        NewWarServiceOptions newWarOptions = NewWarOptions.CurrentValue;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedClan> cachedClans = await dbContext.Clans
            .Where(c =>
                !c.CurrentWar.Added &&
                c.CurrentWar.State != null &&
                c.CurrentWar.State != Rest.Models.WarState.NotInWar &&
                c.Id > _id)
            .OrderBy(c => c.Id)
            .Take(newWarOptions.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _id = cachedClans.Count == newWarOptions.ConcurrentUpdates
            ? cachedClans.Max(c => c.Id)
            : int.MinValue;

        HashSet<string> updatingClans = new();
        int lockSkips = 0;
        long totalSaveMs = 0;

        foreach (CachedClan cachedClan in cachedClans)
            if (Synchronizer.ClanLock.TryAcquire(cachedClan.Tag))
            {
                updatingClans.Add(cachedClan.Tag);

                if (!Synchronizer.WarLock.TryAcquire(cachedClan.CurrentWar.Key))
                {
                    lockSkips++;
                    updatingClans.Remove(cachedClan.Tag);
                    Synchronizer.ClanLock.Release(cachedClan.Tag);
                }
            }

        try
        {
            if (updatingClans.Count == 0)
                return new CycleCounters(cachedClans.Count, 0, lockSkips, totalSaveMs);

            List<CachedWar> cachedWars = await dbContext.Wars
                .AsNoTracking()
                .Where(w => cachedClans.Select(c => c.CurrentWar.PreparationStartTime).Contains(w.PreparationStartTime))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (CachedClan cachedClan in cachedClans)
            {
                if (cachedClan.CurrentWar.Added)
                    continue;

                cachedClan.CurrentWar.Added = true;

                foreach (CachedClan enemyClan in cachedClans)
                    if (enemyClan.CurrentWar.EnemyTag == cachedClan.Tag && enemyClan.CurrentWar.PreparationStartTime == cachedClan.CurrentWar.PreparationStartTime)
                        enemyClan.CurrentWar.Added = true;

                CachedWar? cachedWar = cachedWars.SingleOrDefault(w =>
                    w.PreparationStartTime == cachedClan.CurrentWar.PreparationStartTime &&
                    w.ClanTag == cachedClan.CurrentWar.Content.Clan.Tag &&
                    w.OpponentTag == cachedClan.CurrentWar.Content.Opponent.Tag);

                if (cachedWar != null)
                    continue;

                cachedWar = new CachedWar(cachedClan.CurrentWar);

                dbContext.Wars.Add(cachedWar);

                if (ClanWarAdded != null)
                {
                    Rest.Models.Clan? clan = cachedClan.CurrentWar.Content.Clan.Tag == cachedClan.Tag
                        ? cachedClan.Content
                        : null;

                    Rest.Models.Clan? opponent = cachedClan.CurrentWar.Content.Opponent.Tag == cachedClan.Tag
                        ? cachedClan.Content
                        : null;

                    await ClanWarAdded
                        .Invoke(this, new WarAddedEventArgs(clan, opponent, cachedClan.CurrentWar.Content, cancellationToken))
                        .ConfigureAwait(false);
                }
            }

            var saveSw = System.Diagnostics.Stopwatch.StartNew();
            await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            totalSaveMs += saveSw.ElapsedMilliseconds;

            return new CycleCounters(
                cachedClans.Count,
                updatingClans.Count,
                lockSkips,
                totalSaveMs);
        }
        finally
        {
            foreach (string tag in updatingClans)
            {
                Synchronizer.ClanLock.Release(tag);
                Synchronizer.WarLock.Release(cachedClans.Single(c => c.Tag == tag).CurrentWar.Key);
            }
        }
    }
}