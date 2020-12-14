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
                    if (_clansClient.DownloadCurrentWars == false)
                    {
                        await Task.Delay(Configuration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);

                        continue;
                    }

                    using var scope = Services.CreateScope();

                    CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                    //List<string> clanWarWithLogStatus = await dbContext.ClanWarWithLogStatus
                    //    .Where(w =>
                    //        w.IsWarLogPublic == true &&
                    //        w.DownloadCurrentWar &&
                    //        w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                    //        w.LocalExpiration < DateTime.UtcNow)
                    //    .OrderBy(w => w.ServerExpiration)
                    //    .Take(100)
                    //    .Select(l => l.Tag )
                    //    .ToListAsync()
                    //    .ConfigureAwait(false);

                    List<CachedClanWar> cachedClanWars = await (
                        from cw in dbContext.ClanWars
                        join c in dbContext.Clans on cw.Tag equals c.Tag
                        where c.IsWarLogPublic &&
                            c.DownloadCurrentWar &&
                            cw.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                            cw.LocalExpiration < DateTime.UtcNow
                        orderby cw.ServerExpiration
                        select cw)
                        .Take(Configuration.ConcurrentClanWarDownloads)
                        .ToListAsync();                        

                    List<Task> tasks = new();

                    foreach (CachedClanWar cachedClanWar in cachedClanWars)
                        tasks.Add(MonitorClanWarAsync(cachedClanWar));

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

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

        private async Task MonitorClanWarAsync(CachedClanWar cachedClanWar)
        {
            if (_stopRequestedTokenSource.IsCancellationRequested ||
                _clansClient.UpdatingClanWar.TryAdd(cachedClanWar.Tag, new byte()) == false)
                return;

            try
            {
                string token = await TokenProvider.GetAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                using CancellationTokenSource cts = new CancellationTokenSource(Configuration.HttpRequestTimeOut);

                using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _stopRequestedTokenSource.Token);

                CachedClanWar? fetched = null;

                try
                {
                    fetched = await CachedClanWar.FromCurrentWarResponseAsync(token, cachedClanWar.Tag, _clansClient, _clansApi, linkedCts.Token).ConfigureAwait(false);
                }
                catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException || e is CachedHttpRequestException)
                {
                    if (_stopRequestedTokenSource.IsCancellationRequested)
                        throw;
                    else
                        return;
                }

                if (fetched.Data != null && CachedClanWar.IsNewWar(cachedClanWar, fetched))
                {
                    await _clansClient.InsertNewWarAsync(new CachedWar(fetched)).ConfigureAwait(false);

                    cachedClanWar.Type = fetched.Data.GetWarType();

                    _ = Task.Run(() => _clansClient.TryAddCachedClanWar(fetched.Data.Clans.First(c => c.Key != cachedClanWar.Tag).Key, fetched.ServerExpiration, fetched.LocalExpiration));
                }

                cachedClanWar.UpdateFrom(fetched);
            }
            finally
            {
                _clansClient.UpdatingClanWar.TryRemove(cachedClanWar.Tag, out _);
            }
        }
    }
}