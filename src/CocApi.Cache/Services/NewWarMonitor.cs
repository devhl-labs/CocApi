using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache.Context.CachedItems;
using CocApi.Cache.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public sealed class NewWarMonitor : PerpetualMonitor<NewWarMonitor>
    {
        public event AsyncEventHandler<WarAddedEventArgs>? ClanWarAdded;
        private readonly Synchronizer _synchronizer;
        private readonly IOptions<ClanClientOptions> _options;
        
        public NewWarMonitor(
            CacheDbContextFactoryProvider provider, 
            Synchronizer synchronizer,
            IOptions<ClanClientOptions> options) 
        : base(provider)
        {
            _synchronizer = synchronizer;
            _options = options;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            MonitorOptions options = _options.Value.NewWars;

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    !c.CurrentWar.Added &&
                    c.CurrentWar.State != null &&
                    c.CurrentWar.State != WarState.NotInWar &&
                    c.Id > _id)
                .OrderBy(c => c.Id)
                .Take(options.ConcurrentUpdates)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            _id = cachedClans.Count == options.ConcurrentUpdates
                ? cachedClans.Max(c => c.Id)
                : int.MinValue;

            HashSet<string> updatingClans = new();

            foreach (CachedClan cachedClan in cachedClans)
                if (_synchronizer.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                {
                    updatingClans.Add(cachedClan.Tag);

                    if (!_synchronizer.UpdatingWar.TryAdd(cachedClan.CurrentWar.Key, null))
                    {
                        updatingClans.Remove(cachedClan.Tag);
                        _synchronizer.UpdatingClan.TryRemove(cachedClan.Tag, out _);
                    }
                }

            try
            {
                if (updatingClans.Count == 0)
                    return;

                List<CachedWar> cachedWars = await dbContext.Wars
                    .AsNoTracking()
                    .Where(w => cachedClans.Select(c => c.CurrentWar.PreparationStartTime).Contains(w.PreparationStartTime))
                    .ToListAsync(cancellationToken).ConfigureAwait(false);

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
                        CocApi.Model.Clan? clan = cachedClan.CurrentWar.Content.Clan.Tag == cachedClan.Tag
                            ? cachedClan.Content
                            : null;

                        CocApi.Model.Clan? opponent = cachedClan.CurrentWar.Content.Opponent.Tag == cachedClan.Tag
                            ? cachedClan.Content
                            : null;

                        await ClanWarAdded
                            .Invoke(this, new WarAddedEventArgs(clan, opponent, cachedClan.CurrentWar.Content, cancellationToken))
                            .ConfigureAwait(false);
                    }
                }

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingClans)
                {
                    _synchronizer.UpdatingClan.TryRemove(tag, out _);
                    _synchronizer.UpdatingWar.TryRemove(cachedClans.Single(c => c.Tag == tag).CurrentWar.Key, out _);
                }
            }

            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
        }

        //protected override async Task PollAsync()
        //{
        //    MonitorOptions options = _options.Value.NewWars;

        //    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

        //    List<CachedClan> cachedClans = await dbContext.Clans
        //        .Where(c =>
        //            !c.CurrentWar.Added &&
        //            c.CurrentWar.State != null &&
        //            c.CurrentWar.State != WarState.NotInWar &&
        //            c.Id > _id)
        //        .OrderBy(c => c.Id)
        //        .Take(options.ConcurrentUpdates)
        //        .ToListAsync(cancellationToken)
        //        .ConfigureAwait(false);

        //    _id = cachedClans.Count == options.ConcurrentUpdates
        //        ? cachedClans.Max(c => c.Id)
        //        : int.MinValue;

        //    HashSet<string> updatingClans = new();

        //    foreach (CachedClan cachedClan in cachedClans)
        //        if (_clansClient.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
        //        {
        //            updatingClans.Add(cachedClan.Tag);

        //            if (!_clansClient.UpdatingWar.TryAdd(cachedClan.CurrentWar.Key, null))
        //            {
        //                updatingClans.Remove(cachedClan.Tag);
        //                _clansClient.UpdatingClan.TryRemove(cachedClan.Tag, out _);
        //            }
        //        }

        //    try
        //    {
        //        if (updatingClans.Count == 0)
        //            return;

        //        List<CachedWar> cachedWars = await dbContext.Wars
        //            .AsNoTracking()
        //            .Where(w => cachedClans.Select(c => c.CurrentWar.PreparationStartTime).Contains(w.PreparationStartTime))
        //            .ToListAsync(cancellationToken).ConfigureAwait(false);

        //        foreach (CachedClan cachedClan in cachedClans)
        //        {
        //            if (cachedClan.CurrentWar.Added)
        //                continue;

        //            cachedClan.CurrentWar.Added = true;

        //            foreach (CachedClan enemyClan in cachedClans)
        //                if (enemyClan.CurrentWar.EnemyTag == cachedClan.Tag && enemyClan.CurrentWar.PreparationStartTime == cachedClan.CurrentWar.PreparationStartTime)
        //                    enemyClan.CurrentWar.Added = true;

        //            CachedWar? cachedWar = cachedWars.SingleOrDefault(w =>
        //                w.PreparationStartTime == cachedClan.CurrentWar.PreparationStartTime &&
        //                w.ClanTag == cachedClan.CurrentWar.Content.Clan.Tag &&
        //                w.OpponentTag == cachedClan.CurrentWar.Content.Opponent.Tag);

        //            if (cachedWar != null)
        //                continue;

        //            cachedWar = new CachedWar(cachedClan.CurrentWar);

        //            dbContext.Wars.Add(cachedWar);

        //            CocApi.Model.Clan? clan = cachedClan.CurrentWar.Content.Clan.Tag == cachedClan.Tag
        //                ? cachedClan.Content
        //                : null;

        //            CocApi.Model.Clan? opponent = cachedClan.CurrentWar.Content.Opponent.Tag == cachedClan.Tag
        //                ? cachedClan.Content
        //                : null;

        //            await _clansClient.OnClanWarAddedAsync(new WarAddedEventArgs(clan, opponent, cachedClan.CurrentWar.Content, cancellationToken));
        //        }

        //        await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        //    }
        //    finally
        //    {
        //        foreach (string tag in updatingClans)
        //        {
        //            _clansClient.UpdatingClan.TryRemove(tag, out _);
        //            _clansClient.UpdatingWar.TryRemove(cachedClans.Single(c => c.Tag == tag).CurrentWar.Key, out _);
        //        }
        //    }

        //    if (_id == int.MinValue)
        //        await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
        //    else
        //        await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
        //}
    }
}