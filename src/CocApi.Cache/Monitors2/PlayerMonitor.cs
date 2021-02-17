using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context;
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

        public async Task RunAsync()
        {
            try
            {
                if (_isRunning)
                    return;

                _isRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                Library.OnLog(this, new LogEventArgs(LogLevel.Information, "PlayerMonitor running"));

                while (_stopRequestedTokenSource.IsCancellationRequested == false)
                {
                    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

                    List<Context.CachedItems.CachedPlayer> trackedPlayers = await dbContext.Players
                        .Where(p =>                            
                            p.ExpiresAt < DateTime.UtcNow.AddSeconds(-3) && 
                            p.KeepUntil < DateTime.UtcNow && 
                            p.Download &&
                            p.Id > _id)
                        .OrderBy(p => p.Id)
                        .Take(Library.PlayerMonitorOptions.ConcurrentUpdates)
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    _id = trackedPlayers.Count == Library.PlayerMonitorOptions.ConcurrentUpdates
                        ? trackedPlayers.Max(c => c.Id)
                        : int.MinValue;

                    List<Task> tasks = new();

                    HashSet<string> updatingTags = new();

                    try
                    {
                        foreach(Context.CachedItems.CachedPlayer trackedPlayer in trackedPlayers)
                        {
                            if (!_playersClientBase.UpdatingVillage.TryAdd(trackedPlayer.Tag, trackedPlayer))
                                continue;

                            updatingTags.Add(trackedPlayer.Tag);

                            if (trackedPlayer.Download && trackedPlayer.IsExpired)                    
                                tasks.Add(MonitorPlayerAsync(trackedPlayer, _stopRequestedTokenSource.Token));
                        }

                        try
                        {
                            await Task.WhenAll(tasks).ConfigureAwait(false);
                        }
                        catch (Exception)
                        {
                        }

                        await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);
                    }
                    finally
                    {
                        foreach(string tag in updatingTags)
                            _playersClientBase.UpdatingVillage.TryRemove(tag, out _);
                    }

                    if (_deletedUnmonitoredPlayers < DateTime.UtcNow.AddMinutes(-10))
                    {
                        _deletedUnmonitoredPlayers = DateTime.UtcNow;

                        await DeletePlayersNotMonitoredAsync(dbContext, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                    }

                    await Task.Delay(Library.PlayerMonitorOptions.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                }

                _isRunning = false;
            }
            catch (Exception e)
            {
                _isRunning = false;

                if (_stopRequestedTokenSource.IsCancellationRequested)
                    return;

                if (_stopRequestedTokenSource.IsCancellationRequested == false)
                {
                    Library.OnLog(this, new LogEventArgs(LogLevel.Error, "PlayerMonitor error", e));

                    _ = RunAsync();
                }
            }
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken);

            Library.OnLog(this, new LogEventArgs(LogLevel.Information, "PlayerMonitor stopped"));
        }

        private async Task MonitorPlayerAsync(Context.CachedItems.CachedPlayer cachedPlayer, CancellationToken cancellationToken)
        {
            Context.CachedItems.CachedPlayer fetched = await Context.CachedItems.CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, _playersClientBase, _playersApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && _playersClientBase.HasUpdated(cachedPlayer, fetched))
                _playersClientBase.OnPlayerUpdated(new PlayerUpdatedEventArgs(cachedPlayer.Content, fetched.Content));

            cachedPlayer.UpdateFrom(fetched);
        }

        private async Task DeletePlayersNotMonitoredAsync(CocApiCacheContext dbContext, CancellationToken cancellationToken)
        {            
            List<Context.CachedItems.CachedPlayer> cachedPlayers = await (
                from p in dbContext.Players
                join c in dbContext.Clans on p.ClanTag equals c.Tag
                into p_c
                from c2 in p_c.DefaultIfEmpty()
                where 
                    p.Download == false && 
                    p.ExpiresAt < DateTime.UtcNow.AddMinutes(-10) &&
                    (p.ClanTag == null || (c2 == null || c2.DownloadMembers == false))
                select p
            ).ToListAsync(cancellationToken).ConfigureAwait(false);

            dbContext.RemoveRange(cachedPlayers);

            await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
        }
    }
}