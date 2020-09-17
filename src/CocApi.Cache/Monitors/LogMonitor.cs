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
    internal class LogMonitor : ClientBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        public LogMonitor
            (TokenProvider tokenProvider, ClientConfiguration cacheConfiguration, ClansApi clansApi, ClansClientBase clansClientBase)
            : base(tokenProvider, cacheConfiguration)
        {
            _clansApi = clansApi;
            _clansClient = clansClientBase;
        }

        private int _warId = 0;

        public Task RunAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                _ = MonitorLogsAsync(cancellationToken);
            });

            return Task.CompletedTask;
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken);
        }

        private async Task MonitorLogsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (IsRunning)
                    return;

                IsRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                OnLog(this, new LogEventArgs(nameof(MonitorLogsAsync), LogLevel.Information));

                while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
                {
                    using var scope = _services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    List<Task> tasks = new List<Task>();

                    var cachedWarLogs = await dbContext.ClanWarLogWithLogStatus
                        .AsNoTracking()
                        .Where(w => 
                            w.Id > _warId && 
                            w.IsWarLogPublic == true &&
                            w.DownloadCurrentWar &&
                            w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                            w.LocalExpiration < DateTime.UtcNow)
                        .OrderBy(w => w.Id)
                        .Take(_cacheConfiguration.ConcurrentUpdates)
                        .Select(l => new { l.Id, l.Tag })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedWarLogs.Count; i++)
                    {
                        tasks.Add(MonitorLogAsync(cachedWarLogs[i].Tag));
                    }

                    if (cachedWarLogs.Count < _cacheConfiguration.ConcurrentUpdates)
                        _warId = 0;
                    else
                        _warId = cachedWarLogs.Max(c => c.Id);

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await Task.Delay(_cacheConfiguration.DelayBetweenUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                }

                IsRunning = false;
            }
            catch (Exception e)
            {
                IsRunning = false;

                if (_stopRequestedTokenSource.IsCancellationRequested)
                    return;

                OnLog(this, new ExceptionEventArgs(nameof(MonitorLogsAsync), e));

                if (cancellationToken.IsCancellationRequested == false)
                    _ = RunAsync(cancellationToken);
            }
        }

        private async Task MonitorLogAsync(string tag)
        {
            if (_clansClient.UpdatingClanWar.TryAdd(tag, new byte()) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                CachedClanWarLog cached = await dbContext.WarLogs
                    .Where(w => w.Tag == tag)
                    .FirstAsync(_stopRequestedTokenSource.Token)
                    .ConfigureAwait(false);

                CachedClanWarLog fetched = await CachedClanWarLog
                    .FromClanWarLogResponseAsync(tag, _clansClient, _clansApi, _stopRequestedTokenSource.Token);

                if (fetched.Data != null && _clansClient.HasUpdated(cached, fetched))
                    _clansClient.OnClanWarLogUpdated(cached.Data, fetched.Data);

                cached.UpdateFrom(fetched);

                await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
            }
            finally
            {
                _clansClient.UpdatingClanWar.TryRemove(tag, out _);
            }
        }
    }
}