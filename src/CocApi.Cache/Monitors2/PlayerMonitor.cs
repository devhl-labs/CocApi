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
    internal class PlayerMonitor : MonitorBase
    {
        private readonly PlayersApi _playersApi;
        private readonly PlayersClientBase _playersClientBase;
        private DateTime _deletedUnmonitoredPlayers = DateTime.UtcNow;

        public PlayerMonitor(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs, PlayersApi playersApi, PlayersClientBase playersClientBase) 
            : base(dbContextFactory, dbContextArgs)
        {
            _playersApi = playersApi;
            _playersClientBase = playersClientBase;
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

        //        //_stopRequestedTokenSource = new CancellationTokenSource();

        //        Library.OnLog(this, new LogEventArgs(LogLevel.Information, "running"));

        //        while (_cancellationToken.IsCancellationRequested == false)
        //        {
        //            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

        //            DateTime min = DateTime.MinValue;

        //            List<Context.CachedItems.CachedPlayer> trackedPlayers = await dbContext.Players
        //                .Where(p =>                            
        //                    (p.ExpiresAt ?? min) < DateTime.UtcNow.AddSeconds(-3) && 
        //                    (p.KeepUntil ?? min) < DateTime.UtcNow && 
        //                    p.Download &&
        //                    p.Id > _id)
        //                .OrderBy(p => p.Id)
        //                .Take(Library.Monitors.Players.ConcurrentUpdates)
        //                .ToListAsync(_cancellationToken)
        //                .ConfigureAwait(false);

        //            _id = trackedPlayers.Count == Library.Monitors.Players.ConcurrentUpdates
        //                ? trackedPlayers.Max(c => c.Id)
        //                : int.MinValue;

        //            List<Task> tasks = new();

        //            HashSet<string> updatingTags = new();

        //            try
        //            {
        //                foreach(Context.CachedItems.CachedPlayer trackedPlayer in trackedPlayers)
        //                {
        //                    if (!_playersClientBase.UpdatingVillage.TryAdd(trackedPlayer.Tag, trackedPlayer))
        //                        continue;

        //                    updatingTags.Add(trackedPlayer.Tag);

        //                    if (trackedPlayer.Download && trackedPlayer.IsExpired)                    
        //                        tasks.Add(MonitorPlayerAsync(trackedPlayer));
        //                }

        //                Task updates = Task.WhenAll(tasks);

        //                await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

        //                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

        //                _cancellationToken.ThrowIfCancellationRequested();
        //            }
        //            finally
        //            {
        //                foreach(string tag in updatingTags)
        //                    _playersClientBase.UpdatingVillage.TryRemove(tag, out _);
        //            }

        //            if (_deletedUnmonitoredPlayers < DateTime.UtcNow.AddMinutes(-10))
        //            {
        //                _deletedUnmonitoredPlayers = DateTime.UtcNow;

        //                await DeletePlayersNotMonitoredAsync(dbContext).ConfigureAwait(false);
        //            }

        //            if (_id == int.MinValue)
        //                await Task.Delay(Library.Monitors.Players.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
        //            else
        //                await Task.Delay(Library.Monitors.Players.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        //        }

        //        _isRunning = false;
        //    }
        //    catch (Exception e)
        //    {
        //        _isRunning = false;

        //        if (_cancellationToken.IsCancellationRequested)
        //            return;

        //        Library.OnLog(this, new LogEventArgs(LogLevel.Error, "errored", e));

        //        _ = Task.Run(() => RunAsync(_cancellationToken), CancellationToken.None);                
        //    }
        //}

        //public new async Task StopAsync(CancellationToken cancellationToken)
        //{
        //    await base.BeginShutdown(cancellationToken);

        //    Library.OnLog(this, new LogEventArgs(LogLevel.Information, "PlayerMonitor stopped"));
        //}

        protected override async Task PollAsync()
        {
            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            DateTime min = DateTime.MinValue;

            List<Context.CachedItems.CachedPlayer> trackedPlayers = await dbContext.Players
                .Where(p =>
                    (p.ExpiresAt ?? min) < DateTime.UtcNow.AddSeconds(-3) &&
                    (p.KeepUntil ?? min) < DateTime.UtcNow &&
                    p.Download &&
                    p.Id > _id)
                .OrderBy(p => p.Id)
                .Take(Library.Monitors.Players.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
                .ConfigureAwait(false);

            _id = trackedPlayers.Count == Library.Monitors.Players.ConcurrentUpdates
                ? trackedPlayers.Max(c => c.Id)
                : int.MinValue;

            List<Task> tasks = new();

            HashSet<string> updatingTags = new();

            try
            {
                foreach (Context.CachedItems.CachedPlayer trackedPlayer in trackedPlayers)
                {
                    if (!_playersClientBase.UpdatingVillage.TryAdd(trackedPlayer.Tag, trackedPlayer))
                        continue;

                    updatingTags.Add(trackedPlayer.Tag);

                    if (trackedPlayer.Download && trackedPlayer.IsExpired)
                        tasks.Add(MonitorPlayerAsync(trackedPlayer));
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
                    _playersClientBase.UpdatingVillage.TryRemove(tag, out _);
            }

            if (_deletedUnmonitoredPlayers < DateTime.UtcNow.AddMinutes(-10))
            {
                _deletedUnmonitoredPlayers = DateTime.UtcNow;

                await DeletePlayersNotMonitoredAsync(dbContext).ConfigureAwait(false);
            }

            if (_id == int.MinValue)
                await Task.Delay(Library.Monitors.Players.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(Library.Monitors.Players.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        //public void BeginShutdown()
        //{
        //    _stopRequestedTcs.SetResult(true);

        //    Library.OnLog(this, new LogEventArgs(LogLevel.Information, "stopping"));
        //}

        private async Task MonitorPlayerAsync(Context.CachedItems.CachedPlayer cachedPlayer)
        {
            Context.CachedItems.CachedPlayer fetched = await Context.CachedItems.CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, _playersClientBase, _playersApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && _playersClientBase.HasUpdated(cachedPlayer, fetched))
                await _playersClientBase.OnPlayerUpdatedAsync(new PlayerUpdatedEventArgs(cachedPlayer.Content, fetched.Content, _cancellationToken));

            cachedPlayer.UpdateFrom(fetched);
        }

        private async Task DeletePlayersNotMonitoredAsync(CocApiCacheContext dbContext)
        {
            DateTime min = DateTime.MinValue;

            List<Context.CachedItems.CachedPlayer> cachedPlayers = await (
                from p in dbContext.Players
                join c in dbContext.Clans on p.ClanTag equals c.Tag
                into p_c
                from c2 in p_c.DefaultIfEmpty()
                where 
                    p.Download == false && 
                    (p.ExpiresAt ?? min) < DateTime.UtcNow.AddMinutes(-10) &&
                    (p.ClanTag == null || (c2 == null || c2.DownloadMembers == false))
                select p
            ).ToListAsync(_cancellationToken).ConfigureAwait(false);

            dbContext.RemoveRange(cachedPlayers);

            await dbContext.SaveChangesAsync(_cancellationToken);
        }
    }
}