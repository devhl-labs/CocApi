﻿using System;
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

        public WarMonitor(ClientConfiguration cacheConfiguration, ClansApi clansApi, ClansClientBase clansClientBase) : base(cacheConfiguration)
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

                    var wars = await dbContext.Wars
                        .Where(w =>
                            w.IsFinal == false &&
                            w.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                            w.LocalExpiration < DateTime.UtcNow //&&
                            //w.State < WarState.WarEnded && //todo is there an index on state?
                            //w.EndTime.AddDays(8) > DateTime.UtcNow //todo does this require a new column and a new index?
                            //todo w.StartTime < DateTime.UtcNow.AddHours(-1))  //todo preparation start time < now
                            )
                        .OrderBy(w => w.ServerExpiration)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    for (int i = 0; i < wars.Count; i++)
                        await MonitorWarAsync(wars[i]).ConfigureAwait(false);

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

        private async Task MonitorWarAsync(CachedWar cached)
        {
            if (_stopRequestedTokenSource.IsCancellationRequested ||
                _clansClient.UpdatingWar.TryAdd(cached, new byte()) == false)
                return;

            try
            {
                using var scope = Services.CreateScope();

                CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                List<CachedClanWar>? cachedClanWars = null;

                if (cached.WarTag == null)
                {
                    cachedClanWars = await dbContext.ClanWars
                        .AsNoTracking()
                        .Where(c => cached.ClanTags.Contains(c.Tag))
                        .OrderByDescending(c => c.ServerExpiration)
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    if (cachedClanWars.Count == 2 && cachedClanWars.All(c => c.PreparationStartTime > cached.PreparationStartTime) ||
                        cached.EndTime.AddDays(8) < DateTime.UtcNow)
                    {
                        cached.IsFinal = true;

                        return;
                    }

                    CachedClanWar? cachedClanWar = cachedClanWars
                        .OrderByDescending(c => c.ServerExpiration)
                        .FirstOrDefault(c => c.PreparationStartTime == cached.PreparationStartTime);

                    if (cached.Data != null && cachedClanWar?.Data != null && _clansClient.HasUpdated(cached, cachedClanWar))
                        _clansClient.OnClanWarUpdated(cached.Data, cachedClanWar.Data);

                    if (cachedClanWar != null)
                        cached.UpdateFrom(cachedClanWar);
                }
                else
                {
                    CachedWar? fetched = await CachedWar
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

                cached.IsFinal = cached.State == WarState.WarEnded;

                SendWarAnnouncements(cached, cachedClanWars);
            }
            finally
            {
                _clansClient.UpdatingWar.TryRemove(cached, out _);
            }
        }

        private void SendWarAnnouncements(CachedWar cachedWar, List<CachedClanWar>? cachedClanWars)
        {
            try
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
                    DateTime.UtcNow > cachedWar.EndTime &&
                    DateTime.UtcNow < cachedWar.EndTime.AddHours(24) &&
                    cachedWar.Data.AllAttacksAreUsed() == false &&
                    cachedClanWars != null &&
                    cachedClanWars.All(w => w.Data != null && w.Data.PreparationStartTime != cachedWar.Data.PreparationStartTime))
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
            catch (Exception e)
            {
                _clansClient.OnLog(this, new ExceptionEventArgs(nameof(SendWarAnnouncements), e));

                throw;
            }
        }
    }
}