using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache.Context.CachedItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    internal class NewWarMonitor : MonitorBase
    {
        private readonly ClansClientBase _clansClient;

        public NewWarMonitor(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs, ClansClientBase clansClientBase)
            : base(dbContextFactory, dbContextArgs)
        {
            _clansClient = clansClientBase;
        }

        public async Task RunAsync()
        {
            try
            {
                if (_isRunning)
                    return;

                _isRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                Library.OnLog(this, new LogEventArgs(LogLevel.Information, "NewWarMonitor running"));

                while (_stopRequestedTokenSource.IsCancellationRequested == false)
                {
                    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

                    List<CachedClan> cachedClans = await dbContext.Clans
                        .Where(c =>
                            !c.CurrentWar.Added && 
                            c.CurrentWar.State != null &&
                            c.CurrentWar.State != WarState.NotInWar &&
                            c.Id > _id)
                        .OrderBy(c => c.Id)
                        .Take(Library.Monitors.NewWars.ConcurrentUpdates)
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    _id = cachedClans.Count == Library.Monitors.NewWars.ConcurrentUpdates
                        ? cachedClans.Max(c => c.Id)
                        : int.MinValue;

                    HashSet<string> updatingClans = new();

                    foreach (CachedClan cachedClan in cachedClans)
                        if (_clansClient.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                        {
                            updatingClans.Add(cachedClan.Tag);

                            if (!_clansClient.UpdatingWar.TryAdd(cachedClan.CurrentWar.Key, null))
                                updatingClans.Remove(cachedClan.Tag);
                        }

                    try
                    {
                        if (updatingClans.Count == 0)
                            continue;

                        List<CachedWar> cachedWars = await dbContext.Wars
                            .AsNoTracking()
                            .Where(w => cachedClans.Select(c => c.CurrentWar.PreparationStartTime).Contains(w.PreparationStartTime))
                            .ToListAsync().ConfigureAwait(false);

                        foreach(CachedClan cachedClan in cachedClans)
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

                            await _clansClient.OnClanWarAddedAsync(new WarAddedEventArgs(cachedClan.Content, cachedClan.CurrentWar.Content));
                        }

                        await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);
                    }
                    finally
                    {
                        foreach (string tag in updatingClans)
                        {
                            _clansClient.UpdatingClan.TryRemove(tag, out _);
                            _clansClient.UpdatingWar.TryRemove(cachedClans.Single(c => c.Tag == tag).CurrentWar.Key, out _);
                        }
                    }

                    if (_id == int.MinValue)
                        await Task.Delay(Library.Monitors.NewWars.DelayBetweenBatches, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                    else
                        await Task.Delay(Library.Monitors.NewWars.DelayBetweenBatchUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
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
                    Library.OnLog(this, new LogEventArgs(LogLevel.Error, "NewWarMonitor error", e));

                    _ = RunAsync();
                }
            }
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken).ConfigureAwait(false);

            Library.OnLog(this, new LogEventArgs(LogLevel.Information, "NewWarMonitor stopped"));
        }
    }
}