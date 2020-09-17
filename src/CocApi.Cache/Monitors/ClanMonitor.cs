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

        public ClanMonitor
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
                _ = MonitorClansAsync(cancellationToken);
            });

            return Task.CompletedTask;
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken);
        }

        private async Task MonitorClansAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (IsRunning)
                    return;

                IsRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                OnLog(this, new LogEventArgs(nameof(MonitorClansAsync), LogLevel.Information));

                while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
                {
                    using var scope = _services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    List<Task> tasks = new List<Task>();

                    List<CachedClan> cachedClans = await dbContext.Clans
                        .Where(w => 
                            w.Id > _warId && 
                            w.Download &&
                            w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                            w.LocalExpiration < DateTime.UtcNow)
                        .OrderBy(w => w.Id)
                        .Take(_cacheConfiguration.ConcurrentUpdates)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedClans.Count; i++)
                    {
                        tasks.Add(MonitorClanAsync(cachedClans[i]));
                    }

                    if (cachedClans.Count < _cacheConfiguration.ConcurrentUpdates)
                        _warId = 0;
                    else
                        _warId = cachedClans.Max(c => c.Id);

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);

                    await Task.Delay(_cacheConfiguration.DelayBetweenUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                }

                IsRunning = false;
            }
            catch (Exception e)
            {
                IsRunning = false;

                if (_stopRequestedTokenSource.IsCancellationRequested)
                    return;

                OnLog(this, new ExceptionEventArgs(nameof(MonitorClansAsync), e));

                if (cancellationToken.IsCancellationRequested == false)
                    _ = RunAsync(cancellationToken);
            }
        }

        private async Task MonitorClanAsync(CachedClan cached)
        {
            if (_clansClient.UpdatingClan.TryAdd(cached.Tag, new byte()) == false)
                return;

            try
            {
                CachedClan fetched = await CachedClan
                    .FromClanResponseAsync(cached.Tag, _clansClient, _clansApi, _stopRequestedTokenSource.Token);

                if (cached.Data != null && fetched.Data != null && _clansClient.HasUpdated(cached, fetched))
                    _clansClient.OnClanUpdated(cached.Data, fetched.Data);

                cached.UpdateFrom(fetched);
            }
            finally
            {
                _clansClient.UpdatingClan.TryRemove(cached.Tag, out _);
            }
        }
    }
}