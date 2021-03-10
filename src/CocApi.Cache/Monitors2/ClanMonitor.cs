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
    internal class ClanMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        public ClanMonitor(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs, ClansApi clansApi, ClansClientBase clansClientBase)
            : base(dbContextFactory, dbContextArgs)
        {
            _clansApi = clansApi;
            _clansClient = clansClientBase;
        }

//        public async Task RunAsync(CancellationToken cancellationToken)
//        {
//            _cancellationToken = cancellationToken;

//            _cancellationToken.Register(BeginShutdown);

//            try
//            {
//                if (_isRunning)
//                    return;

//                _isRunning = true;

//                Library.OnLog(this, new LogEventArgs(LogLevel.Information, "running"));

//                while (_cancellationToken.IsCancellationRequested == false)
//                {
//                    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

//                    DateTime expires = DateTime.UtcNow.AddSeconds(-3);

//                    DateTime min = DateTime.MinValue;

//                    List<Context.CachedItems.CachedClan> cachedClans = await dbContext.Clans
//                        .Where(c =>
//                            (
//((c.ExpiresAt ?? min) < expires            && (c.KeepUntil ?? min) < DateTime.UtcNow              && c.Download) ||
//((c.Group.ExpiresAt ?? min) < expires      && (c.Group.KeepUntil ?? min) < DateTime.UtcNow        && c.Group.Download) ||
//((c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < DateTime.UtcNow   && c.CurrentWar.Download && c.IsWarLogPublic != false) ||
//((c.WarLog.ExpiresAt ?? min) < expires     && (c.WarLog.KeepUntil ?? min) < DateTime.UtcNow       && c.WarLog.Download     && c.IsWarLogPublic != false) 
//                            )
//                            &&
//                            c.Id > _id)
//                        .OrderBy(c => c.Id)
//                        .Take(Library.Monitors.Clans.ConcurrentUpdates)
//                        .ToListAsync(_cancellationToken)
//                        .ConfigureAwait(false);

//                    _id = cachedClans.Count == Library.Monitors.Clans.ConcurrentUpdates
//                        ? cachedClans.Max(c => c.Id)
//                        : int.MinValue;

//                    List<Task> tasks = new();

//                    HashSet<string> updatingTags = new();

//                    try
//                    {

//                        foreach (Context.CachedItems.CachedClan cachedClan in cachedClans)
//                        {
//                            if (!_clansClient.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
//                                continue;

//                            updatingTags.Add(cachedClan.Tag);

//                            tasks.Add(UpdateAsync(cachedClan));
//                        }

//                        Task updates = Task.WhenAll(tasks);

//                        await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

//                        await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

//                        _cancellationToken.ThrowIfCancellationRequested();
//                    }
//                    finally
//                    {
//                        foreach(string tag in updatingTags)
//                            _clansClient.UpdatingClan.TryRemove(tag, out _);
//                    }

//                    if (_id == int.MinValue)
//                        await Task.Delay(Library.Monitors.Clans.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
//                    else
//                        await Task.Delay(Library.Monitors.Clans.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
//                }

//                _isRunning = false;
//            }
//            catch (Exception e)
//            {
//                _isRunning = false;

//                if (_cancellationToken.IsCancellationRequested)
//                    return;

//                Library.OnLog(this, new LogEventArgs(LogLevel.Information, "errored", e));

//                _ = Task.Run(() => RunAsync(_cancellationToken));                
//            }
//        }

        //public new async Task StopAsync(CancellationToken cancellationToken)
        //{
        //    await base.BeginShutdown(cancellationToken);

        //    Library.OnLog(this, new LogEventArgs(LogLevel.Information, "ClanMonitor stopped"));
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

            List<Context.CachedItems.CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    (
((c.ExpiresAt ?? min) < expires && (c.KeepUntil ?? min) < DateTime.UtcNow && c.Download) ||
((c.Group.ExpiresAt ?? min) < expires && (c.Group.KeepUntil ?? min) < DateTime.UtcNow && c.Group.Download) ||
((c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < DateTime.UtcNow && c.CurrentWar.Download && c.IsWarLogPublic != false) ||
((c.WarLog.ExpiresAt ?? min) < expires && (c.WarLog.KeepUntil ?? min) < DateTime.UtcNow && c.WarLog.Download && c.IsWarLogPublic != false)
                    )
                    &&
                    c.Id > _id)
                .OrderBy(c => c.Id)
                .Take(Library.Monitors.Clans.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
                .ConfigureAwait(false);

            _id = cachedClans.Count == Library.Monitors.Clans.ConcurrentUpdates
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

                    tasks.Add(UpdateAsync(cachedClan));
                }

                await Task.WhenAll(tasks);

                //Task updates = Task.WhenAll(tasks);

                //await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

                _cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                foreach (string tag in updatingTags)
                    _clansClient.UpdatingClan.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(Library.Monitors.Clans.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(Library.Monitors.Clans.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateAsync(Context.CachedItems.CachedClan cachedClan)
        {
            try
            {
                List<Task> tasks = new();

                if (cachedClan.Download && cachedClan.IsExpired)
                    tasks.Add(MonitorClanAsync(cachedClan));

                if (cachedClan.CurrentWar.Download && cachedClan.CurrentWar.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorClanWarAsync(cachedClan));

                if (cachedClan.WarLog.Download && cachedClan.WarLog.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorWarLogAsync(cachedClan));

                if (cachedClan.Group.Download && cachedClan.Group.IsExpired)
                    tasks.Add(MonitorGroupAsync(cachedClan));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        private async Task MonitorClanAsync(Context.CachedItems.CachedClan cachedClan)
        {
            Context.CachedItems.CachedClan fetched = await Context.CachedItems.CachedClan.FromClanResponseAsync(cachedClan.Tag, _clansClient, _clansApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && _clansClient.HasUpdated(cachedClan, fetched))
                await _clansClient.OnClanUpdatedAsync(new ClanUpdatedEventArgs(cachedClan.Content, fetched.Content, _cancellationToken));

            cachedClan.UpdateFrom(fetched);
        }

        private async Task MonitorClanWarAsync(Context.CachedItems.CachedClan cachedClan)
        {
            Context.CachedItems.CachedClanWar? fetched = await Context.CachedItems.CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, _clansClient, _clansApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && Context.CachedItems.CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }

        private async Task MonitorWarLogAsync(Context.CachedItems.CachedClan cachedClan)
        {
            Context.CachedItems.CachedClanWarLog fetched = await Context.CachedItems.CachedClanWarLog.FromClanWarLogResponseAsync(cachedClan.Tag, _clansClient, _clansApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && _clansClient.HasUpdated(cachedClan.WarLog, fetched))
                await _clansClient.OnClanWarLogUpdatedAsync(new ClanWarLogUpdatedEventArgs(cachedClan.WarLog.Content, fetched.Content, cachedClan.Content, _cancellationToken));

            cachedClan.WarLog.UpdateFrom(fetched);
        }

        private async Task MonitorGroupAsync(Context.CachedItems.CachedClan cachedClan)
        {
            Context.CachedItems.CachedClanWarLeagueGroup? fetched = await Context.CachedItems.CachedClanWarLeagueGroup
                .FromClanWarLeagueGroupResponseAsync(cachedClan.Tag, _clansClient, _clansApi, _cancellationToken)
                .ConfigureAwait(false);

            if (fetched.Content != null && _clansClient.HasUpdated(cachedClan.Group, fetched))
            {
                await _clansClient.OnClanWarLeagueGroupUpdatedAsync(new ClanWarLeagueGroupUpdatedEventArgs(cachedClan.Group.Content, fetched.Content, cachedClan.Content, _cancellationToken));

                cachedClan.Group.Added = false;                
            }

            cachedClan.Group.UpdateFrom(fetched);
        }
    }
}