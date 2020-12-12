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
    internal class WarLogMonitor : ClientBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        public WarLogMonitor
            (TokenProvider tokenProvider, ClientConfiguration cacheConfiguration, ClansApi clansApi, ClansClientBase clansClientBase)
            : base(tokenProvider, cacheConfiguration)
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
                    if (_clansClient.DownloadWarLog == false)
                    {
                        await Task.Delay(Configuration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);

                        continue;
                    }

                    using var scope = Services.CreateScope();

                    CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                    //var clanWarLogsWithLogStatus = await dbContext.ClanWarLogWithLogStatus
                    //    .Where(w =>
                    //        w.IsWarLogPublic &&
                    //        w.DownloadCurrentWar &&
                    //        w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                    //        w.LocalExpiration < DateTime.UtcNow)
                    //    .OrderBy(w => w.ServerExpiration)
                    //    .Take(1000)
                    //    .Select(l => l.Tag)
                    //    .ToListAsync()
                    //    .ConfigureAwait(false);

                    List<CachedClanWarLog> cachedLogs = await (
                        from l in dbContext.WarLogs
                        join c in dbContext.Clans on l.Tag equals c.Tag
                        where c.IsWarLogPublic && c.DownloadCurrentWar && l.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) && l.LocalExpiration < DateTime.UtcNow
                        orderby l.ServerExpiration
                        select l)
                        .Take(Configuration.ConcurrentWarLogDownloads)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    List<Task> tasks = new();

                    foreach (CachedClanWarLog cachedLog in cachedLogs)
                        tasks.Add(MonitorLogAsync(cachedLog));

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                    //for (int i = 0; i < clanWarLogsWithLogStatus.Count; i++)                    
                    //    await MonitorLogAsync(clanWarLogsWithLogStatus[i]);                    

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

        private async Task MonitorLogAsync(CachedClanWarLog cached)
        {
            if (_stopRequestedTokenSource.IsCancellationRequested ||
                _clansClient.UpdatingClanWar.TryAdd(cached.Tag, new byte()) == false)
                return;

            try
            {
                //using var scope = Services.CreateScope();

                //CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                //CachedClanWarLog cached = await dbContext.WarLogs
                //    .Where(w => w.Tag == tag)
                //    .FirstAsync(_stopRequestedTokenSource.Token)
                //    .ConfigureAwait(false);

                string token = await TokenProvider.GetAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                using CancellationTokenSource cts = new CancellationTokenSource(Configuration.HttpRequestTimeOut);

                using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _stopRequestedTokenSource.Token);

                CachedClanWarLog fetched = await CachedClanWarLog
                    .FromClanWarLogResponseAsync(token, cached.Tag, _clansClient, _clansApi, linkedCts.Token).ConfigureAwait(false);

                if (fetched.Data != null && _clansClient.HasUpdated(cached, fetched))
                    _clansClient.OnClanWarLogUpdated(cached.Data, fetched.Data);

                cached.UpdateFrom(fetched);

                //await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);
            }
            finally
            {
                _clansClient.UpdatingClanWar.TryRemove(cached.Tag, out _);
            }
        }
    }
}