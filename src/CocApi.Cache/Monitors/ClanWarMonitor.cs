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
using CocApi.Cache.View;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    internal class ClanWarMonitor : ClientBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        public ClanWarMonitor
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
                _ = MonitorClanWarsAsync(cancellationToken);
            });

            return Task.CompletedTask;
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken);
        }

        private async Task MonitorClanWarsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (IsRunning)
                    return;

                IsRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                OnLog(this, new LogEventArgs(nameof(MonitorClanWarsAsync), LogLevel.Information));

                while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
                {
                    using var scope = _services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    List<Task> tasks = new List<Task>();

                    var cachedWarLogs = await dbContext.ClanWarWithLogStatus
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
                        tasks.Add(MonitorClanWarAsync(cachedWarLogs[i].Tag));
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

                OnLog(this, new ExceptionEventArgs(nameof(MonitorClanWarsAsync), e));

                if (cancellationToken.IsCancellationRequested == false)
                    _ = RunAsync(cancellationToken);
            }
        }

        private async Task MonitorClanWarAsync(string tag)
        {
            if (_clansClient.UpdatingClanWar.TryAdd(tag, new byte()) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                CachedClanWar? cachedClanWar = await dbContext.ClanWars
                    .Where(w => w.Tag == tag)
                    .FirstAsync(_stopRequestedTokenSource.Token)
                    .ConfigureAwait(false);

                CachedClanWar fetched = await CachedClanWar
                    .FromCurrentWarResponseAsync(tag, _clansClient, _clansApi, _stopRequestedTokenSource.Token);

                if (fetched.Data != null && _clansClient.IsNewWar(cachedClanWar, fetched))
                {
                    await _clansClient.InsertNewWarAsync(new CachedWar(fetched));

                    cachedClanWar.Type = fetched.Data.WarType;
                }

                cachedClanWar.UpdateFrom(fetched);

                await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
            }
            finally
            {
                _clansClient.UpdatingClanWar.TryRemove(tag, out _);
            }
        }
    }
}