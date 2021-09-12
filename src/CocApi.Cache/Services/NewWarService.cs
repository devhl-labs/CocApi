using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache.Services
{
    public sealed class NewWarService : PerpetualService<NewWarService>
    {
        internal event AsyncEventHandler<WarAddedEventArgs>? ClanWarAdded;
        
        
        internal Synchronizer Synchronizer { get; }
        internal IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }

        
        
        public NewWarService(
            CacheDbContextFactoryProvider provider, 
            Synchronizer synchronizer,
            IOptions<CacheOptions> options) 
        : base(provider, options.Value.NewWars.DelayBeforeExecution, options.Value.NewWars.DelayBetweenExecutions)
        {
            Instantiated = Library.EnsureSingleton(Instantiated);
            IsEnabled = options.Value.NewWars.Enabled;
            Synchronizer = synchronizer;
            Options = options;
        }



        private protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            ServiceOptions options = Options.Value.NewWars;

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
                if (Synchronizer.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                {
                    updatingClans.Add(cachedClan.Tag);

                    if (!Synchronizer.UpdatingWar.TryAdd(cachedClan.CurrentWar.Key, null))
                    {
                        updatingClans.Remove(cachedClan.Tag);
                        Synchronizer.UpdatingClan.TryRemove(cachedClan.Tag, out _);
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
                    Synchronizer.UpdatingClan.TryRemove(tag, out _);
                    Synchronizer.UpdatingWar.TryRemove(cachedClans.Single(c => c.Tag == tag).CurrentWar.Key, out _);
                }
            }
        }
    }
}