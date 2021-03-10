using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    internal sealed class ActiveWarMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        public ActiveWarMonitor(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs, ClansApi clansApi, ClansClientBase clansClientBase)
            : base(dbContextFactory, dbContextArgs)
        {
            _clansApi = clansApi;
            _clansClient = clansClientBase;
        }

        //public async Task RunAsync(CancellationToken cancellationToken)
        //{
        //    _cancellationToken = cancellationToken;

        //    _cancellationToken.Register(BeginShutdown);

        //    try
        //    {
        //        if (_isRunning)
        //            return;

        //        _isRunning = true;

        //        Library.OnLog(this, new LogEventArgs(LogLevel.Information, "running"));

        //        while (_cancellationToken.IsCancellationRequested == false)
        //        {
        //            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

        //            DateTime expires = DateTime.UtcNow.AddSeconds(-3);

        //            DateTime min = DateTime.MinValue;

        //            List<Context.CachedItems.CachedClan> cachedClans = await
        //                (
        //                    from c in dbContext.Clans
        //                    join w in dbContext.Wars on c.Tag equals w.ClanTag
        //                    where 
        //                        !c.CurrentWar.Download && 
        //                        (c.Download == false || c.Download && c.IsWarLogPublic == true) &&
        //                        (c.CurrentWar.ExpiresAt ?? min) < expires && 
        //                        (c.CurrentWar.KeepUntil ?? min) < DateTime.UtcNow && 
        //                        c.Id > _id  &&
        //                        !w.IsFinal
        //                    orderby c.Id
        //                    select c
        //                ).Union(
        //                    from c in dbContext.Clans
        //                    join w in dbContext.Wars on c.Tag equals w.OpponentTag
        //                    where
        //                        !c.CurrentWar.Download &&
        //                        (c.Download == false || c.Download && c.IsWarLogPublic == true) &&
        //                        (c.CurrentWar.ExpiresAt ?? min) < expires &&
        //                        (c.CurrentWar.KeepUntil ?? min) < DateTime.UtcNow &&
        //                        c.Id > _id &&
        //                        !w.IsFinal
        //                    orderby c.Id
        //                    select c
        //                )
        //                .Distinct()
        //                .Take(Library.Monitors.ActiveWars.ConcurrentUpdates)
        //                .ToListAsync(_cancellationToken)
        //                .ConfigureAwait(false);

        //            _id = cachedClans.Count == Library.Monitors.ActiveWars.ConcurrentUpdates
        //                ? cachedClans.Max(c => c.Id)
        //                : int.MinValue;

        //            List<Task> tasks = new();

        //            HashSet<string> updatingTags = new();

        //            try
        //            {
        //                foreach (Context.CachedItems.CachedClan cachedClan in cachedClans)
        //                {
        //                    if (!_clansClient.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
        //                        continue;

        //                    updatingTags.Add(cachedClan.Tag);
                            
        //                    tasks.Add(MonitorClanWarAsync(cachedClan));
        //                }

        //                Task updates = Task.WhenAll(tasks);

        //                await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

        //                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false); // if cancelled how do i avoid a race condition here?

        //                _cancellationToken.ThrowIfCancellationRequested();
        //            }
        //            finally
        //            {
        //                foreach(string tag in updatingTags)
        //                    _clansClient.UpdatingClan.TryRemove(tag, out _);
        //            }
                    
        //            if (_id == int.MinValue)
        //                await Task.Delay(Library.Monitors.ActiveWars.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
        //            else
        //                await Task.Delay(Library.Monitors.ActiveWars.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        //        }

        //        _isRunning = false;
        //    }
        //    catch (Exception e)
        //    {
        //        _isRunning = false;

        //        if (_cancellationToken.IsCancellationRequested)
        //            return;

        //        Library.OnLog(this, new LogEventArgs(LogLevel.Error, "errored", e));

        //        _ = Task.Run(() => RunAsync(_cancellationToken), CancellationToken.None);     // todo StopAsync wont work if this fires           
        //    }
        //}

        //public void BeginShutdown()
        //{
        //    _stopRequestedTcs.SetResult(true);

        //    Library.OnLog(this, new LogEventArgs(LogLevel.Information, "stopping"));
        //}

        protected override async Task PollAsync()
        {
            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            DateTime expires = DateTime.UtcNow.AddSeconds(-3);

            DateTime min = DateTime.MinValue;

            List<Context.CachedItems.CachedClan> cachedClans = await
                (
                    from c in dbContext.Clans
                    join w in dbContext.Wars on c.Tag equals w.ClanTag
                    where
                        !c.CurrentWar.Download &&
                        (c.Download == false || c.Download && c.IsWarLogPublic == true) &&
                        (c.CurrentWar.ExpiresAt ?? min) < expires &&
                        (c.CurrentWar.KeepUntil ?? min) < DateTime.UtcNow &&
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
                        (c.CurrentWar.KeepUntil ?? min) < DateTime.UtcNow &&
                        c.Id > _id &&
                        !w.IsFinal
                    orderby c.Id
                    select c
                )
                .Distinct()
                .Take(Library.Monitors.ActiveWars.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
                .ConfigureAwait(false);

            _id = cachedClans.Count == Library.Monitors.ActiveWars.ConcurrentUpdates
                ? cachedClans.Max(c => c.Id)
                : int.MinValue;

            List<Task> tasks = new();

            HashSet<string> updatingTags = new();

            try
            {
                foreach (Context.CachedItems.CachedClan cachedClan in cachedClans)
                {
                    if (!_clansClient.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                        continue;

                    updatingTags.Add(cachedClan.Tag);

                    tasks.Add(MonitorClanWarAsync(cachedClan));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false); // if cancelled how do i avoid a race condition here?

                _cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                foreach (string tag in updatingTags)
                    _clansClient.UpdatingClan.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(Library.Monitors.ActiveWars.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(Library.Monitors.ActiveWars.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task MonitorClanWarAsync(Context.CachedItems.CachedClan cachedClan)
        {
            Context.CachedItems.CachedClanWar fetched = await Context.CachedItems.CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, _clansClient, _clansApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && Context.CachedItems.CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }
    }
}