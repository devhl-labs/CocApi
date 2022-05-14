using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.IApis;
using CocApi.Cache.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CocApi.Rest.Client;
using CocApi.Cache.Options;

namespace CocApi.Cache.Services
{
    public sealed class ActiveWarService : ServiceBase
    {
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

            List<Task> tasks = new();

            HashSet<string> updatingTags = new();

            try
            {
                foreach (CachedClan cachedClan in cachedClans)
                {
                    if (!Synchronizer.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                        continue;

                    updatingTags.Add(cachedClan.Tag);

                    tasks.Add(MonitorClanWarAsync(cachedClan, cancellationToken));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingTags)
                    Synchronizer.UpdatingClan.TryRemove(tag, out _);
            }
        }

        private async Task MonitorClanWarAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            IClansApi clansApi = ApiFactory.Create<IClansApi>();

            CachedClanWar fetched = await CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, Ttl, clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }
    }
}