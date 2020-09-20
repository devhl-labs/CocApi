using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
                        await Task.Delay(ClientConfiguration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);

                        continue;
                    }

                    using var scope = Services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    List<Task> tasks = new List<Task>();

                    var cachedWarLogs = await dbContext.ClanWarLeagueGroupWithStatus
                        .Where(w =>
                            w.Id > _id &&
                            w.DownloadCwl == true &&
                            w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                            w.LocalExpiration < DateTime.UtcNow)
                        .OrderBy(w => w.Id)
                        .Take(ClientConfiguration.ConcurrentUpdates)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedWarLogs.Count; i++)
                        tasks.Add(MonitorCwlAsync(cachedWarLogs[i].Tag));

                    if (cachedWarLogs.Count < ClientConfiguration.ConcurrentUpdates)
                        _id = 0;
                    else
                        _id = cachedWarLogs.Max(c => c.Id);

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


        private readonly ConcurrentDictionary<string, byte> _updatingGroup = new ConcurrentDictionary<string, byte>();

        private async Task MonitorCwlAsync(string tag)
        {
            if (_updatingGroup.TryAdd(tag, new byte()) == false)
                return;

            try
            {
                using var scope = Services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                CachedClanWarLeagueGroup cached = await dbContext.Groups
                    .Where(w => w.Tag == tag)
                    .FirstAsync(_stopRequestedTokenSource.Token)
                    .ConfigureAwait(false);

                CachedClanWarLeagueGroup fetched = await CachedClanWarLeagueGroup
                    .FromClanWarLeagueGroupResponseAsync(tag, _clansClient, _clansApi, _stopRequestedTokenSource.Token);

                if (fetched.Data != null && _clansClient.HasUpdated(cached, fetched))
                    _clansClient.OnClanWarLeagueGroupUpdated(cached.Data, fetched.Data);

                cached.UpdateFrom(fetched);

                List<Task> tasks = new List<Task>();

                List<string> downloadWarTags = new List<string>();

                if (cached.Data == null || cached.Season.Month < DateTime.Now.Month)
                {
                    await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                    return;
                }

                try
                {
                    foreach (var round in cached.Data.Rounds)
                        foreach (string warTag in round.WarTags)
                        {
                            if (_updatingWarTags.TryAdd(warTag, new byte()))
                            {
                                downloadWarTags.Add(warTag);

                                tasks.Add(ReturnNewWarAsync(tag, cached.Season, warTag, _stopRequestedTokenSource.Token));
                            }
                        }

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    foreach (Task<CachedWar?> cachedWarTask in tasks)
                        if (await cachedWarTask is CachedWar cachedWar)
                            dbContext.Wars.Add(cachedWar);

                    await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
                }
                finally
                {
                    foreach (string warTag in downloadWarTags)
                        _updatingWarTags.TryRemove(warTag, out _);
                }
            }
            finally
            {
                _updatingGroup.TryRemove(tag, out _);
            }
        }


        private static readonly ConcurrentDictionary<string, byte> _updatingWarTags = new ConcurrentDictionary<string, byte>();

        private async Task<CachedWar?> ReturnNewWarAsync(string tag, DateTime season, string warTag, CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Token.ThrowIfCancellationRequested();

            using var scope = Services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedWar? cachedWar = await dbContext.Wars
                .FirstOrDefaultAsync(w => w.WarTag == warTag)
                .ConfigureAwait(false);

            if (cachedWar != null)
                return null;

            CachedWar fetched = await CachedWar.FromClanWarLeagueWarResponseAsync(warTag, season, _clansClient, _clansApi, cancellationToken);

            if (fetched.ClanTags.Any(c => c == tag) == false)
                return null;

            return fetched;
        }
    }
}