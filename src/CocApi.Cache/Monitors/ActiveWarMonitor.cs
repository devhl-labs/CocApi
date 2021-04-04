using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    internal sealed class ActiveWarMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;
        private readonly IOptions<ClientOptions> _options;

        public ActiveWarMonitor(CacheDbContextFactoryProvider provider, ClansApi clansApi, ClansClientBase clansClient, IOptions<ClientOptions> options) 
            : base(provider)
        {
            _clansApi = clansApi;
            _clansClient = clansClient;
            _options = options;
        }

        protected override async Task PollAsync()
        {
            MonitorOptions options = _options.Value.ActiveWars;

            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

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
                .Take(options.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
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
                    if (!_clansClient.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                        continue;

                    updatingTags.Add(cachedClan.Tag);

                    tasks.Add(MonitorClanWarAsync(cachedClan));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingTags)
                    _clansClient.UpdatingClan.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task MonitorClanWarAsync(CachedClan cachedClan)
        {
            CachedClanWar fetched = await CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, _clansClient, _clansApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }
    }
}