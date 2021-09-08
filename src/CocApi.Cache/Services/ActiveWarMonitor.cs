using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using CocApi.Cache.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public sealed class ActiveWarMonitor : PerpetualMonitor<ActiveWarMonitor>
    {
        private readonly ClansApi _clansApi;
        private readonly Synchronizer _synchronizer;
        private readonly TimeToLiveProvider _ttl;
        private readonly IOptions<ClanClientOptions> _options;

        public ActiveWarMonitor(
            CacheDbContextFactoryProvider provider, 
            ClansApi clansApi, 
            Synchronizer synchronizer,
            TimeToLiveProvider ttl,
            IOptions<ClanClientOptions> options) 
            : base(provider)
        {
            _clansApi = clansApi;
            _synchronizer = synchronizer;
            _ttl = ttl;
            _options = options;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            MonitorOptions options = _options.Value.ActiveWars;

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

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
                    if (!_synchronizer.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
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
                    _synchronizer.UpdatingClan.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
        }

        //protected override async Task PollAsync()
        //{
        //    MonitorOptions options = _options.Value.ActiveWars;

        //    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

        //    List<CachedClan> cachedClans = await
        //        (
        //            from c in dbContext.Clans
        //            join w in dbContext.Wars on c.Tag equals w.ClanTag
        //            where
        //                !c.CurrentWar.Download &&
        //                (c.Download == false || c.Download && c.IsWarLogPublic == true) &&
        //                (c.CurrentWar.ExpiresAt ?? min) < expires &&
        //                (c.CurrentWar.KeepUntil ?? min) < now &&
        //                c.Id > _id &&
        //                !w.IsFinal
        //            orderby c.Id
        //            select c
        //        ).Union(
        //            from c in dbContext.Clans
        //            join w in dbContext.Wars on c.Tag equals w.OpponentTag
        //            where
        //                !c.CurrentWar.Download &&
        //                (c.Download == false || c.Download && c.IsWarLogPublic == true) &&
        //                (c.CurrentWar.ExpiresAt ?? min) < expires &&
        //                (c.CurrentWar.KeepUntil ?? min) < now &&
        //                c.Id > _id &&
        //                !w.IsFinal
        //            orderby c.Id
        //            select c
        //        )
        //        .Distinct()
        //        .Take(options.ConcurrentUpdates)
        //        .ToListAsync(cancellationToken)
        //        .ConfigureAwait(false);

        //    _id = cachedClans.Count == options.ConcurrentUpdates
        //        ? cachedClans.Max(c => c.Id)
        //        : int.MinValue;

        //    List<Task> tasks = new();

        //    HashSet<string> updatingTags = new();

        //    try
        //    {
        //        foreach (CachedClan cachedClan in cachedClans)
        //        {
        //            if (!_clansClient.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
        //                continue;

        //            updatingTags.Add(cachedClan.Tag);

        //            tasks.Add(MonitorClanWarAsync(cachedClan));
        //        }

        //        await Task.WhenAll(tasks);

        //        await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        //    }
        //    finally
        //    {
        //        foreach (string tag in updatingTags)
        //            _clansClient.UpdatingClan.TryRemove(tag, out _);
        //    }

        //    if (_id == int.MinValue)
        //        await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
        //    else
        //        await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
        //}

        private async Task MonitorClanWarAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClanWar fetched = await CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, _ttl, _clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }
    }
}