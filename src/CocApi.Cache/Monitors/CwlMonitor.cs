using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    internal class CwlMonitor : ClientBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        public CwlMonitor
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
                    if (_clansClient.DownloadCwl == false)
                    {
                        await Task.Delay(Configuration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);

                        continue;
                    }

                    using var scope = Services.CreateScope();

                    CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                    //var cachedWarLogs = await dbContext.ClanWarLeagueGroupWithStatus
                    //    .Where(w =>
                    //        w.DownloadCwl == true &&
                    //        w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                    //        w.LocalExpiration < DateTime.UtcNow)
                    //    .OrderBy(w => w.ServerExpiration)
                    //    .Take(10)
                    //    .ToListAsync()
                    //    .ConfigureAwait(false);

                    List<CachedClanWarLeagueGroup> cachedGroup = await (
                        from g in dbContext.Groups
                        join c in dbContext.Clans on g.Tag equals c.Tag
                        where c.DownloadCwl && g.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) && g.LocalExpiration < DateTime.UtcNow
                        orderby g.ServerExpiration
                        select g)
                        .Take(Configuration.ConcurrentCwlDownloads)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    List<Task> tasks = new();

                    foreach (CachedClanWarLeagueGroup group in cachedGroup)
                        tasks.Add(MonitorCwlAsync(group, dbContext));

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


        private readonly ConcurrentDictionary<string, byte> _updatingGroup = new ConcurrentDictionary<string, byte>();

        private async Task MonitorCwlAsync(CachedClanWarLeagueGroup cached, CacheContext dbContext)
        {
            if (_stopRequestedTokenSource.IsCancellationRequested ||
                _updatingGroup.TryAdd(cached.Tag, new byte()) == false)
                return;

            try
            {
                //using var scope = Services.CreateScope();

                //CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                //CachedClanWarLeagueGroup cached = await dbContext.Groups
                //    .Where(w => w.Tag == tag)
                //    .FirstAsync(_stopRequestedTokenSource.Token)
                //    .ConfigureAwait(false);

                string token = await TokenProvider.GetAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                using CancellationTokenSource cts = new CancellationTokenSource(Configuration.HttpRequestTimeOut);

                using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _stopRequestedTokenSource.Token);

                CachedClanWarLeagueGroup? fetched = null;

                try
                {
                    fetched = await CachedClanWarLeagueGroup
                        .FromClanWarLeagueGroupResponseAsync(token, cached.Tag, _clansClient, _clansApi, linkedCts.Token).ConfigureAwait(false);
                }
                catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException)
                {
                    if (_stopRequestedTokenSource.IsCancellationRequested)
                        throw;
                    else
                        return;
                }

                if (fetched.Data != null && _clansClient.HasUpdated(cached, fetched))
                    _clansClient.OnClanWarLeagueGroupUpdated(cached.Data, fetched.Data);

                cached.UpdateFrom(fetched);

                List<Task> tasks = new List<Task>();

                HashSet<string> downloadWarTags = new HashSet<string>();

                if (cached.Data == null || cached.Season.Month < DateTime.Now.Month)
                {
                    //await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                    return;
                }

                try
                {
                    foreach (var round in cached.Data.Rounds)
                        foreach (string warTag in round.WarTags.Where(w => w != "#0"))                        
                            if (_updatingWarTags.TryAdd(warTag, new byte()))
                            {
                                downloadWarTags.Add(warTag);

                                tasks.Add(ReturnNewWarAsync(cached.Tag, cached.Season, warTag));
                            }

                    try
                    {
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                    }
                    catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException)
                    {
                    }

                    foreach (Task<CachedWar?> cachedWarTask in tasks)
                        if (cachedWarTask.IsCompletedSuccessfully && cachedWarTask.Result is CachedWar cachedWar && cachedWar.Data != null)
                        {
                            dbContext.Wars.Add(cachedWar);
                            _clansClient.OnClanWarAdded(cachedWar.Data);
                        }

                    //await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);
                }
                finally
                {
                    foreach (string warTag in downloadWarTags)
                        _updatingWarTags.TryRemove(warTag, out _);
                }
            }
            finally
            {
                _updatingGroup.TryRemove(cached.Tag, out _);
            }
        }


        private static readonly ConcurrentDictionary<string, byte> _updatingWarTags = new ConcurrentDictionary<string, byte>();

        private async Task<CachedWar?> ReturnNewWarAsync(string tag, DateTime season, string warTag)
        {
            _stopRequestedTokenSource.Token.ThrowIfCancellationRequested();

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedWar? cachedWar = await dbContext.Wars
                .FirstOrDefaultAsync(w => w.WarTag == warTag && w.Season == season)
                .ConfigureAwait(false);

            if (cachedWar != null)
                return null;

            string token = await TokenProvider.GetAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

            using CancellationTokenSource cts = new CancellationTokenSource(Configuration.HttpRequestTimeOut);

            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _stopRequestedTokenSource.Token);

            CachedWar? fetched = await CachedWar
                .FromClanWarLeagueWarResponseAsync(token, warTag, season, _clansClient, _clansApi, linkedCts.Token).ConfigureAwait(false);

            if (fetched.ClanTags.Any(c => c == tag) == false)
                return null;

            return fetched;
        }
    }
}