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
        private readonly PlayersClientBase? _playersClientBase;
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        private ConcurrentDictionary<string, byte> _updatingClan = new ConcurrentDictionary<string, byte>();

        public ClanMonitor
            (PlayersClientBase? playersClientBase, TokenProvider tokenProvider, ClientConfiguration cacheConfiguration, ClansApi clansApi, ClansClientBase clansClientBase)
            : base(tokenProvider, cacheConfiguration)
        {
            _playersClientBase = playersClientBase;
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
                    using var scope = Services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    List<Task> tasks = new List<Task>();

                    List<CachedClan> cachedClans = await dbContext.Clans
                        .Where(w =>
                            w.Id > _id &&
                            (w.Download || w.DownloadMembers) &&
                            w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                            w.LocalExpiration < DateTime.UtcNow)
                        .OrderBy(w => w.Id)
                        .Take(ClientConfiguration.ConcurrentUpdates)
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedClans.Count; i++)
                    {
                        if (cachedClans[i].Download)
                            tasks.Add(MonitorClanAsync(cachedClans[i]));

                        if (cachedClans[i].DownloadMembers && _clansClient.DownloadMembers && _playersClientBase != null)
                            tasks.Add(MonitorMembersAsync(cachedClans[i]));
                    }

                    if (cachedClans.Count < ClientConfiguration.ConcurrentUpdates)
                        _id = 0;
                    else
                        _id = cachedClans.Max(c => c.Id);

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);

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

        private async Task MonitorClanAsync(CachedClan cached)
        {
            if (_updatingClan.TryAdd(cached.Tag, new byte()) == false)
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
                _updatingClan.TryRemove(cached.Tag, out _);
            }
        }

        private async Task MonitorMembersAsync(CachedClan cached)
        {
            if (cached.Data == null)
                return;

            List<Task> tasks = new List<Task>();

            foreach (Model.ClanMember? member in cached.Data.Members)
                tasks.Add(MonitorMemberAsync(member.Tag));

            await Task.WhenAll(tasks);
        }

        private async Task MonitorMemberAsync(string tag)
        {
            if (_playersClientBase == null)
                return;

            using var scope = Services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedPlayer? cached = await dbContext.Players
                .FirstOrDefaultAsync(p => p.Tag == tag, _stopRequestedTokenSource.Token)
                .ConfigureAwait(false);

            if (cached == null)
            {
                cached = new CachedPlayer(tag);

                dbContext.Players.Add(cached);
            }

            if (cached.ServerExpiration > DateTime.UtcNow.AddSeconds(3) || cached.LocalExpiration > DateTime.UtcNow)
                return;

            await _playersClientBase.PlayerMontitor.UpdatePlayerAsync(cached);

            await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);
        }
    }
}