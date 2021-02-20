using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    internal class NewCwlWarMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public NewCwlWarMonitor(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs, ClansApi clansApi, ClansClientBase clansClientBase)
            : base(dbContextFactory, dbContextArgs)
        {
            _clansApi = clansApi;
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

                Library.OnLog(this, new LogEventArgs(LogLevel.Information, "NewCwlWarMonitor running"));

                while (_stopRequestedTokenSource.IsCancellationRequested == false)
                {
                    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

                    List<CachedClan> cachedClans = await dbContext.Clans
                        .Where(c =>
                            c.CurrentWar.Download && 
                            !c.Group.Added &&
                            !string.IsNullOrWhiteSpace(c.Group.RawContent) &&
                            c.Id > _id)
                        .OrderBy(c => c.Id)
                        .Take(Library.Monitors.NewCwlWars.ConcurrentUpdates)
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    _id = cachedClans.Count == Library.Monitors.NewCwlWars.ConcurrentUpdates
                        ? cachedClans.Max(c => c.Id)
                        : int.MinValue;

                    List<Task> tasks = new();

                    Dictionary<string, DateTime> updatingTags = new();

                    foreach (CachedClan cachedClan in cachedClans)
                    {
                        cachedClan.Group.Added = true;

                        foreach (var round in cachedClan.Group.Content.Rounds)
                            foreach (string tag in round.WarTags.Where(t => t != "#0"))
                                if (!_clansClient.UpdatingCwlWar.TryAdd(tag, null))
                                    updatingTags.TryAdd(tag, cachedClan.Group.Season.Value);
                    }

                    try
                    {
                        if (updatingTags.Count == 0)
                            continue;

                        List<CachedWar> cachedWars = await dbContext.Wars
                            .Where(w => !string.IsNullOrWhiteSpace(w.WarTag) && updatingTags.Keys.Contains(w.WarTag))
                            .ToListAsync(_stopRequestedTokenSource.Token)
                            .ConfigureAwait(false);

                        foreach (var kvp in updatingTags.Where(t => !cachedWars.Any(w => w.WarTag == t.Key && w.Season == t.Value)))
                            tasks.Add(InsertNewCwlWarAsync(kvp.Key, kvp.Value, dbContext, _stopRequestedTokenSource.Token));

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
                        foreach(string tag in updatingTags.Keys)
                            _clansClient.UpdatingCwlWar.TryRemove(tag, out _);
                    }

                    if (_id == int.MinValue)
                        await Task.Delay(Library.Monitors.NewCwlWars.DelayBetweenBatches, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                    else
                        await Task.Delay(Library.Monitors.NewCwlWars.DelayBetweenBatchUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
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
                    Library.OnLog(this, new LogEventArgs(LogLevel.Error, "NewCwlWarMonitor error", e));

                    _ = RunAsync();
                }
            }
        }

        private async Task InsertNewCwlWarAsync(string tag, DateTime season, CocApiCacheContext dbContext, CancellationToken cancellationToken)
        {
            CachedWar cachedWar = await CachedWar.FromClanWarLeagueWarResponseAsync(tag, season, _clansClient, _clansApi, cancellationToken).ConfigureAwait(false);

            if (cachedWar.Content != null)
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    dbContext.Wars.Add(cachedWar);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken);

            Library.OnLog(this, new LogEventArgs(LogLevel.Information, "NewCwlWarMonitor stopped"));
        }
    }
}