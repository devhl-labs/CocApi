//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using CocApi.Api;
//using CocApi.Cache.Models;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;

//namespace CocApi.Cache
//{
//    internal class ClanMembersMonitor : ClientBase
//    {
//        private readonly PlayersClientBase _playersClientBase;
//        private readonly ClansApi _clansApi;
//        private readonly ClansClientBase _clansClient;

//        public ClanMembersMonitor(PlayersClientBase playersClientBase, ClientConfiguration configuration, ClansApi clansApi, ClansClientBase clansClientBase) 
//            : base(configuration)
//        {
//            _playersClientBase = playersClientBase;
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
//                    if (!_clansClient.DownloadMembers)
//                    {
//                        await Task.Delay(Configuration.DelayBetweenTasks);

//                        continue;
//                    }

//                    using var scope = Services.CreateScope();

//                    CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

//                    List<CachedClan> cachedClans = await dbContext.Clans
//                        .OrderBy(c => c.Id)
//                        .Where(c => c.Id > id && c.DownloadMembers)
//                        .Take(Configuration.ConcurrentClanDownloads)
//                        .AsNoTracking()
//                        .ToListAsync(_stopRequested)
//                        .ConfigureAwait(false);

//                    id = cachedClans.Count < Configuration.ConcurrentClanDownloads
//                        ? int.MinValue
//                        : cachedClans.Max(c => c.Id);

//                    foreach (CachedClan cachedClan in cachedClans)                      
//                        await MonitorMembersAsync(cachedClan);                        

//                    await Task.Delay(Configuration.DelayBetweenTasks, _stopRequested).ConfigureAwait(false);
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

//        private async Task MonitorMembersAsync(CachedClan cached)
//        {
//            if (_stopRequestedTokenSource.IsCancellationRequested || cached.Data == null)
//                return;

//            List<Task> tasks = new List<Task>();

//            using var scope = Services.CreateScope();

//            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

//            List<CachedPlayer> players = await dbContext.Players.Where(p => cached.Data.Members.Select(m => m.Tag).Contains(p.Tag)).ToListAsync();

//            foreach(Model.ClanMember member in cached.Data.Members.Where(m => !players.Any(p => p.Tag == m.Tag)))
//            {
//                CachedPlayer player = new CachedPlayer(member.Tag);

//                dbContext.Players.Add(player);

//                players.Add(player);
//            }

//            foreach (Model.ClanMember? member in cached.Data.Members)
//                tasks.Add(MonitorMemberAsync(players.Single(p => p.Tag == member.Tag)));

//            await Task.WhenAll(tasks);

//            await dbContext.SaveChangesAsync(_stopRequested);
//        }

//        private async Task MonitorMemberAsync(CachedPlayer cached)
//        {
//            if (cached.ServerExpiration > DateTime.UtcNow.AddSeconds(3) || cached.LocalExpiration > DateTime.UtcNow)
//                return;

//            await _playersClientBase.PlayerMontitor.UpdatePlayerAsync(cached);
//        }
//    }
//}