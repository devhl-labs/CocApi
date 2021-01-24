using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    internal class ClanMonitor : ClientBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        private readonly ConcurrentDictionary<string, byte> _updatingClan = new ConcurrentDictionary<string, byte>();

        private DateTime _deletedUnmonitoredPlayers = DateTime.UtcNow;

        public ClanMonitor(ClientConfiguration configuration, ClansApi clansApi, ClansClientBase clansClientBase) : base(configuration)
        {
            _clansApi = clansApi;
            _clansClient = clansClientBase;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_isRunning)
                    return;

                _isRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                _clansClient.OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

                while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
                {
                    using var scope = Services.CreateScope();

                    CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                    List<CachedClan> cachedClans = await dbContext.Clans
                        .Where(w =>
                            w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                            w.LocalExpiration < DateTime.UtcNow)
                        .OrderBy(w => w.ServerExpiration)
                        .Take(Configuration.ConcurrentClanDownloads)
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    List<Task> tasks = new();

                    foreach(CachedClan cachedClan in cachedClans)                    
                        tasks.Add(MonitorClanAsync(cachedClan));

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                    if (_deletedUnmonitoredPlayers < DateTime.UtcNow.AddMinutes(-10))
                    {
                        _deletedUnmonitoredPlayers = DateTime.UtcNow;

                        tasks = new List<Task>
                        {
                            DeleteUnmonitoredPlayersNotInAClan(),

                            DeletePlayersInClansNotMonitored()
                        };

                        await Task.WhenAll(tasks);
                    }

                    await Task.Delay(Configuration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                }

                _isRunning = false;
            }
            catch (Exception e)
            {
                _isRunning = false;

                if (_stopRequestedTokenSource.IsCancellationRequested)
                    return;

                _clansClient.OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

                if (cancellationToken.IsCancellationRequested == false)
                    _ = RunAsync(cancellationToken);
            }
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken);

            _clansClient.OnLog(this, new LogEventArgs(nameof(StopAsync), LogLevel.Information));
        }

        private async Task MonitorClanAsync(CachedClan cached)
        {
            if (_stopRequestedTokenSource.IsCancellationRequested ||
                _updatingClan.TryAdd(cached.Tag, new byte()) == false)
                return;

            try
            {
                CachedClan fetched = await CachedClan.FromClanResponseAsync(cached.Tag, _clansClient, _clansApi, _stopRequestedTokenSource.Token);

                if (cached.Data != null && fetched.Data != null && _clansClient.HasUpdated(cached, fetched))
                    _clansClient.OnClanUpdated(cached.Data, fetched.Data);

                cached.UpdateFrom(fetched);
            }
            finally
            {
                _updatingClan.TryRemove(cached.Tag, out _);
            }
        }

        private async Task DeleteUnmonitoredPlayersNotInAClan()
        {
            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            // delete any player who is not being monitored so stale items dont get stuck in cache
            List<CachedPlayer> cachedPlayers = await dbContext.Players
                .Where(p => 
                    p.ClanTag == null && 
                    p.Download == false && 
                    p.ServerExpiration < DateTime.UtcNow.AddMinutes(-10))
                .ToListAsync(_stopRequestedTokenSource.Token);

            dbContext.RemoveRange(cachedPlayers);

            await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
        }

        private async Task DeletePlayersInClansNotMonitored()
        {
            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            await dbContext.Database.ExecuteSqlRawAsync(@"
delete 
from players
where Tag in (
 select p.tag
 from players p
 join clans as c on p.clantag = c.tag
 where c.downloadmembers = false and p.download = false and p.serverexpiration < Datetime('now', '-10 minutes', 'utc')
)"
            );
        }
    }
}