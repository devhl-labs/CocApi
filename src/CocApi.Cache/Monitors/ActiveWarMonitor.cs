using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    internal class ActiveWarMonitor : ClientBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        public ActiveWarMonitor
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
                    if (_clansClient.DownloadCurrentWars == false && _clansClient.DownloadCwl == false)
                    {
                        await Task.Delay(Configuration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);

                        continue;
                    }

                    using var scope = Services.CreateScope();

                    CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                    var warWithLogStatus = await dbContext.WarWithLogStatus
                        .Where(w =>
                            w.IsFinal == false &&
                            w.IsWarLogPublic != false &&
                            w.DownloadCurrentWar != true)
                        .OrderBy(w => w.ServerExpiration)
                        .Take(1000)
                        .Select(w => new { w.ClanTag, w.OpponentTag })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    for (int i = 0; i < warWithLogStatus.Count; i++)
                    {
                        await MonitorActiveWarAsync(warWithLogStatus[i].ClanTag);
                        await MonitorActiveWarAsync(warWithLogStatus[i].OpponentTag);
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

        private async Task MonitorActiveWarAsync(string clanTag)
        {
            if (_stopRequestedTokenSource.IsCancellationRequested ||
                _clansClient.UpdatingClanWar.TryAdd(clanTag, new byte()) == false)
                return;

            try
            {
                using var scope = Services.CreateScope();

                CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                CachedClanWar? cachedClanWar = await dbContext.ClanWars
                    .FirstOrDefaultAsync(w => w.Tag == clanTag, _stopRequestedTokenSource.Token)
                    .ConfigureAwait(false);

                if (cachedClanWar == null)
                {
                    cachedClanWar = new CachedClanWar(clanTag);

                    dbContext.ClanWars.Add(cachedClanWar);
                }

                if (cachedClanWar.IsLocallyExpired() == false || cachedClanWar.IsServerExpired() == false)
                    return;

                using CancellationTokenSource cts = new CancellationTokenSource(Configuration.HttpRequestTimeOut);

                CachedClanWar? fetched = null;

                try
                {
                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _stopRequestedTokenSource.Token);

                    fetched = await CachedClanWar.FromCurrentWarResponseAsync(clanTag, _clansClient, _clansApi, linkedCts.Token);
                }
                catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException)
                {
                    if (_stopRequestedTokenSource.IsCancellationRequested)
                        throw;
                    else
                        return;
                }

                if (fetched.Data != null && CachedClanWar.IsNewWar(cachedClanWar, fetched))
                    await _clansClient.InsertNewWarAsync(new CachedWar(fetched));

                cachedClanWar.UpdateFrom(fetched);

                await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);

            }
            finally
            {
                _clansClient.UpdatingClanWar.TryRemove(clanTag, out _);
            }
        }
    }
}