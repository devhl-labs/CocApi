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
                        await Task.Delay(ClientConfiguration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);

                        continue;
                    }

                    using var scope = Services.CreateScope();

                    CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                    List<Task> tasks = new List<Task>();

                    var cachedWars = await dbContext.WarWithLogStatus
                        .AsNoTracking()
                        .Where(w =>
                            w.Id > _id &&
                            w.State < WarState.WarEnded &&
                            w.IsFinal == false &&
                            (w.IsWarLogPublic == null || w.IsWarLogPublic == true))
                        .OrderBy(w => w.Id)
                        .Take(ClientConfiguration.ConcurrentUpdates)
                        .Select(w => new { w.Id, w.ClanTag, w.OpponentTag })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedWars.Count; i++)
                    {
                        tasks.Add(MonitorActiveWarAsync(cachedWars[i].ClanTag));
                        tasks.Add(MonitorActiveWarAsync(cachedWars[i].OpponentTag));
                    }

                    if (cachedWars.Count < ClientConfiguration.ConcurrentUpdates)
                        _id = 0;
                    else
                        _id = cachedWars.Max(c => c.Id);

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await Task.Delay(ClientConfiguration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);
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
            if (_clansClient.UpdatingClanWar.TryAdd(clanTag, new byte()) == false)
                return;

            try
            {
                using var scope = Services.CreateScope();

                CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                CachedClanWar? cachedClanWar = await dbContext.ClanWars
                    .Where(w => w.Tag == clanTag)
                    .FirstOrDefaultAsync(_stopRequestedTokenSource.Token)
                    .ConfigureAwait(false);

                if (cachedClanWar == null)
                {
                    cachedClanWar = new CachedClanWar(clanTag);

                    dbContext.ClanWars.Add(cachedClanWar);
                }

                if (cachedClanWar.IsLocallyExpired() == false || cachedClanWar.IsServerExpired() == false)
                    return; 

                CachedClanWar fetched = await CachedClanWar
                    .FromCurrentWarResponseAsync(clanTag, _clansClient, _clansApi, _stopRequestedTokenSource.Token);

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