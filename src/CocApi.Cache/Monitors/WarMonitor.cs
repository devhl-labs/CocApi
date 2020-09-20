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
    internal class WarMonitor : ClientBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        public WarMonitor
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
                    using var scope = Services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    List<Task> tasks = new List<Task>();

                    var cachedWarLogs = await dbContext.Wars
                        .Where(w =>
                            w.Id > _id &&
                            w.IsFinal == false &&
                            w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                            w.LocalExpiration < DateTime.UtcNow)
                        .OrderBy(w => w.Id)
                        .Take(ClientConfiguration.ConcurrentUpdates)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedWarLogs.Count; i++)                    
                        tasks.Add(MonitorWarAsync(cachedWarLogs[i]));                    

                    if (cachedWarLogs.Count < ClientConfiguration.ConcurrentUpdates)
                        _id = 0;
                    else
                        _id = cachedWarLogs.Max(c => c.Id);

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

        private async Task MonitorWarAsync(CachedWar cached)
        {
            if (_clansClient.UpdatingWar.TryAdd(cached, new byte()) == false)
                return;

            try
            {
                using var scope = Services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                if (cached.WarTag == null)
                {
                    List<CachedClanWar> cachedClanWars = await dbContext.ClanWars
                        .AsNoTracking()
                        .Where(c => cached.ClanTags.Any(tag => tag == c.Tag))
                        .OrderByDescending(c => c.ServerExpiration)
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    if ((cachedClanWars.All(c => c.PreparationStartTime != cached.PreparationStartTime) && cached.EndTime < DateTime.UtcNow) ||
                        cachedClanWars.All(c => c.StatusCode == HttpStatusCode.Forbidden && cached.EndTime.AddDays(8) < DateTime.UtcNow))
                    {
                        cached.IsFinal = true;

                        return;
                    }

                    CachedClanWar? cachedClanWar = cachedClanWars
                        .FirstOrDefault(c => c.PreparationStartTime == cached.PreparationStartTime);

                    if (cached.Data != null && cachedClanWar?.Data != null && _clansClient.HasUpdated(cached, cachedClanWar))
                        _clansClient.OnClanWarUpdated(cached.Data, cachedClanWar.Data);

                    if (cachedClanWar != null)
                        cached.UpdateFrom(cachedClanWar);
                }
                else
                {
                    CachedWar fetched = await CachedWar
                        .FromClanWarLeagueWarResponseAsync(
                            cached.WarTag, cached.Season.Value, _clansClient, _clansApi, _stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    if (cached.Data != null && 
                        fetched.Data != null && 
                        cached.Season == fetched.Season && 
                        _clansClient.HasUpdated(cached, fetched))
                        _clansClient.OnClanWarUpdated(cached.Data, fetched.Data);

                    cached.UpdateFrom(fetched);
                }

                SendWarAnnouncements(cached);
            }
            finally
            {
                _clansClient.UpdatingWar.TryRemove(cached, out _);
            }
        }

        private void SendWarAnnouncements(CachedWar cachedWar)
        {
            if (cachedWar.Data == null)
                return;

            if (cachedWar.Announcements.HasFlag(Announcements.WarStartingSoon) == false &&
                DateTime.UtcNow > cachedWar.Data.StartTime.AddHours(-1) &&
                DateTime.UtcNow < cachedWar.Data.StartTime)
            {
                cachedWar.Announcements |= Announcements.WarStartingSoon;
                _clansClient.OnClanWarStartingSoon(cachedWar.Data);
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
                DateTime.UtcNow > cachedWar.Data.EndTime.AddHours(-1) &&
                DateTime.UtcNow < cachedWar.Data.EndTime)
            {
                cachedWar.Announcements |= Announcements.WarEndingSoon;
                _clansClient.OnClanWarEndingSoon(cachedWar.Data);
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEndNotSeen) == false &&
                cachedWar.State != WarState.WarEnded &&
                cachedWar.IsFinal == true &&
                DateTime.UtcNow > cachedWar.EndTime &&
                DateTime.UtcNow.Day == cachedWar.EndTime.Day &&
                cachedWar.Data.AllAttacksAreUsed() == false)
            {
                cachedWar.Announcements |= Announcements.WarEndNotSeen;
                _clansClient.OnClanWarEndNotSeen(cachedWar.Data);
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
                cachedWar.EndTime < DateTime.UtcNow &&
                cachedWar.EndTime.Day == DateTime.UtcNow.Day)
            {
                cachedWar.Announcements |= Announcements.WarEnded;
                _clansClient.OnClanWarEnded(cachedWar.Data);
            }
        }
    }
}