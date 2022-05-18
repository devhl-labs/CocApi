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

namespace CocApi.Cache.Services
{
    public sealed class NewWarService : ServiceBase
    {
        internal event AsyncEventHandler<WarAddedEventArgs>? ClanWarAdded;
        
        
        internal Synchronizer Synchronizer { get; }
        public IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }

        
        
        public NewWarService(
            ILogger<NewWarService> logger,
            IServiceScopeFactory scopeFactory,
            Synchronizer synchronizer,
            IOptions<CacheOptions> options)
        : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.NewWars))
        {
            Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
            Synchronizer = synchronizer;
            Options = options;
        }



        protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            NewWarServiceOptions options = Options.Value.NewWars;

            using var scope = ScopeFactory.CreateScope();

            CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    !c.CurrentWar.Added &&
                    c.CurrentWar.State != null &&
                    c.CurrentWar.State != Rest.Models.WarState.NotInWar &&
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