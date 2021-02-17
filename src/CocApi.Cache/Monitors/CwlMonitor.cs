//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using CocApi.Api;
//using CocApi.Cache.Models;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;

//namespace CocApi.Cache
//{
//    internal class CwlMonitor : ClientBase
//    {
//        private readonly ClansApi _clansApi;
//        private readonly ClansClientBase _clansClient;

//        public CwlMonitor(ClientConfiguration cacheConfiguration, ClansApi clansApi, ClansClientBase clansClientBase) : base(cacheConfiguration)
//        {
//            _clansApi = clansApi;
//            _clansClient = clansClientBase;
//        }

//        public async Task RunAsync(CancellationToken cancellationToken)
//        {
//            try
//            {
//                if (_isRunning)
//                    return;

//                _isRunning = true;

//                _stopRequestedTokenSource = new CancellationTokenSource();

//                _clansClient.OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

//                int id = int.MinValue;

//                while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
//                {
//                    if (_clansClient.DownloadCwl == false)
//                    {
//                        await Task.Delay(Configuration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);

//                        continue;
//                    }

//                    using var scope = Services.CreateScope();

//                    CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

//                    //var cachedWarLogs = await dbContext.ClanWarLeagueGroupWithStatus
//                    //    .Where(w =>
//                    //        w.DownloadCwl == true &&
//                    //        w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
//                    //        w.LocalExpiration < DateTime.UtcNow)
//                    //    .OrderBy(w => w.ServerExpiration)
//                    //    .Take(10)
//                    //    .ToListAsync()
//                    //    .ConfigureAwait(false);

//                    List<CachedClanWarLeagueGroup> cachedGroup = await (
//                        from g in dbContext.Groups
//                        join c in dbContext.Clans on g.Tag equals c.Tag
//                        where c.DownloadCwl && g.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) && g.LocalExpiration < DateTime.UtcNow && g.Id > id
//                        orderby g.Id
//                        select g)
//                        .Take(Configuration.ConcurrentCwlDownloads)
//                        .ToListAsync()
//                        .ConfigureAwait(false);

//                    id = cachedGroup.Count == Configuration.ConcurrentClanDownloads
//                        ? cachedGroup.Max(g => g.Id)
//                        : int.MinValue;

//                    List<Task> tasks = new();

//                    foreach (CachedClanWarLeagueGroup group in cachedGroup)
//                        tasks.Add(MonitorCwlAsync(group, dbContext));

//                    await Task.WhenAll(tasks).ConfigureAwait(false);

//                    await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

//                    await Task.Delay(Configuration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);
//                }

//                _isRunning = false;
//            }
//            catch (Exception e)
//            {
//                _isRunning = false;

//                if (_stopRequestedTokenSource.IsCancellationRequested)
//                    return;

//                _clansClient.OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

//                if (cancellationToken.IsCancellationRequested == false)
//                    _ = RunAsync(cancellationToken);
//            }
//        }

//        public new async Task StopAsync(CancellationToken cancellationToken)
//        {
//            _stopRequestedTokenSource.Cancel();

//            await base.StopAsync(cancellationToken);

//            _clansClient.OnLog(this, new LogEventArgs(nameof(StopAsync), LogLevel.Information));
//        }

//        private readonly ConcurrentDictionary<string, byte> _updatingGroup = new ConcurrentDictionary<string, byte>();
//        private static readonly ConcurrentDictionary<string, byte> _updatingWarTags = new ConcurrentDictionary<string, byte>();

//        private async Task MonitorCwlAsync(CachedClanWarLeagueGroup cached, CacheContext dbContext)
//        {
//            if (_stopRequestedTokenSource.IsCancellationRequested ||
//                _updatingGroup.TryAdd(cached.Tag, new byte()) == false)
//                return;

//            try
//            {
//                CachedClanWarLeagueGroup? fetched = await CachedClanWarLeagueGroup
//                    .FromClanWarLeagueGroupResponseAsync(cached.Tag, _clansClient, _clansApi, _stopRequestedTokenSource.Token)
//                    .ConfigureAwait(false);

//                if (fetched.Data != null && _clansClient.HasUpdated(cached, fetched))
//                    _clansClient.OnClanWarLeagueGroupsUpdated(cached.Data, fetched.Data);

//                cached.UpdateFrom(fetched);

//                List<Task> tasks = new List<Task>();

//                HashSet<string> downloadWarTags = new HashSet<string>();

//                if (cached.Data == null || cached.Season.Value.Month < DateTime.Now.Month)
//                    return;                

//                try
//                {
//                    foreach (var round in cached.Data.Rounds)
//                        foreach (string warTag in round.WarTags.Where(w => w != "#0"))                        
//                            if (_updatingWarTags.TryAdd(warTag, new byte()))
//                            {
//                                downloadWarTags.Add(warTag);

//                                tasks.Add(ReturnNewWarOrDefaultAsync(cached.Tag, cached.Season.Value, warTag));
//                            }

//                    await Task.WhenAll(tasks).ConfigureAwait(false);

//                    foreach (Task<CachedWar?> cachedWarTask in tasks)
//                        if (cachedWarTask.IsCompletedSuccessfully && cachedWarTask.Result is CachedWar cachedWar && cachedWar.Data != null)
//                        {
//                            dbContext.Wars.Add(cachedWar);
//                            _clansClient.OnClanWarAdded(cachedWar.Data);
//                        }
//                }
//                finally
//                {
//                    foreach (string warTag in downloadWarTags)
//                        _updatingWarTags.TryRemove(warTag, out _);
//                }
//            }
//            catch (Exception e)
//            {
//                _clansClient.OnLog(this, new LogEventArgs(nameof(MonitorCwlAsync), LogLevel.Debug, $"Error updating group:{cached.Tag};\n{e.Message};\n{e.InnerException?.Message}"));
//            }
//            finally
//            {
//                _updatingGroup.TryRemove(cached.Tag, out _);
//            }
//        }

//        private async Task<CachedWar?> ReturnNewWarOrDefaultAsync(string tag, DateTime season, string warTag)
//        {
//            if (_stopRequestedTokenSource.IsCancellationRequested)
//                return null;

//            try
//            {
//                using var scope = Services.CreateScope();

//                CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

//                CachedWar? cachedWar = await dbContext.Wars
//                    .FirstOrDefaultAsync(w => w.WarTag == warTag && w.Season == season)
//                    .ConfigureAwait(false);

//                if (cachedWar != null)
//                    return null;

//                CachedWar fetched = await CachedWar
//                    .FromClanWarLeagueWarResponseAsync(warTag, season, _clansClient, _clansApi, _stopRequestedTokenSource.Token)
//                    .ConfigureAwait(false);

//                if (fetched.ClanTags.Any(c => c == tag) == false)
//                    return null;

//                return fetched;
//            }
//            catch (Exception e)
//            {
//                _clansClient.OnLog(this, new LogEventArgs(nameof(ReturnNewWarOrDefaultAsync), LogLevel.Debug, $"Error updating cwl war:{tag}\n{e.Message}\n{e.InnerException?.Message}"));

//                return null;
//            }
//        }
//    }
//}