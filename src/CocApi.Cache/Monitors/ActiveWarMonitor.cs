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

        private int _warId = 0;

        public Task RunAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                _ = MonitorActiveWarsAsync(cancellationToken);
            });

            return Task.CompletedTask;
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken);
        }

        private async Task MonitorActiveWarsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (IsRunning)
                    return;

                IsRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                OnLog(this, new LogEventArgs(nameof(MonitorActiveWarsAsync), LogLevel.Information));

                while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
                {
                    using var scope = _services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    List<Task> tasks = new List<Task>();

                    var cachedWars = await dbContext.WarWithLogStatus
                        .AsNoTracking()
                        .Where(w => 
                            w.Id > _warId && 
                            w.State < WarState.WarEnded && 
                            w.IsFinal == false &&
                            (w.IsWarLogPublic == null || w.IsWarLogPublic == true))
                        .OrderBy(w => w.Id)
                        .Take(_cacheConfiguration.ConcurrentUpdates)
                        .Select(w => new {w.Id, w.ClanTag, w.OpponentTag })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedWars.Count; i++)
                    {
                        tasks.Add(MonitorActiveWarAsync(cachedWars[i].ClanTag));
                        tasks.Add(MonitorActiveWarAsync(cachedWars[i].OpponentTag));
                    }

                    if (cachedWars.Count < _cacheConfiguration.ConcurrentUpdates)
                        _warId = 0;
                    else
                        _warId = cachedWars.Max(c => c.Id);

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

                OnLog(this, new ExceptionEventArgs(nameof(MonitorActiveWarsAsync), e));

                if (cancellationToken.IsCancellationRequested == false)
                    _ = RunAsync(cancellationToken);
            }
        }

        private async Task MonitorActiveWarAsync(string clanTag)
        {
            if (_clansClient.UpdatingClanWar.TryAdd(clanTag, new byte()) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

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
                _clansClient.UpdatingClanWar.TryRemove(clanTag, out _);
            }
        }
    }
}