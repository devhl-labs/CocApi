﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using CocApi.Cache.Context.CachedItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    internal class WarMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;
        private readonly HashSet<string> _unmonitoredClans = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public WarMonitor(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs, ClansApi clansApi, ClansClientBase clansClientBase)
            : base(dbContextFactory, dbContextArgs)
        {
            _clansApi = clansApi;
            _clansClient = clansClientBase;
        }

        protected override async Task PollAsync()
        {
            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            DateTime expires = DateTime.UtcNow.AddSeconds(-3);

            DateTime min = DateTime.MinValue;

            List<CachedWar> cachedWars = await dbContext.Wars
                .Where(w =>
                    (w.ExpiresAt ?? min) < expires &&
                    (w.KeepUntil ?? min) < DateTime.UtcNow &&
                    !w.IsFinal &&
                    w.Id > _id)
                .OrderBy(c => c.Id)
                .Take(Library.Monitors.Wars.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
                .ConfigureAwait(false);

            _id = cachedWars.Count == Library.Monitors.Wars.ConcurrentUpdates
                ? cachedWars.Max(c => c.Id)
                : int.MinValue;

            HashSet<string> tags = new();

            foreach (CachedWar cachedWar in cachedWars)
            {
                tags.Add(cachedWar.ClanTag);
                tags.Add(cachedWar.OpponentTag);
            }

            List<CachedClan> allCachedClans = await dbContext.Clans
                .Where(c => tags.Contains(c.Tag))
                .AsNoTracking()
                .ToListAsync(_cancellationToken)
                .ConfigureAwait(false);

            List<Task> tasks = new();

            HashSet<string> updatingWar = new();

            HashSet<string> updatingCwlWar = new();

            try
            {
                foreach (CachedWar cachedWar in cachedWars)
                {
                    List<CachedClan> cachedClans = allCachedClans.Where(c => c.Tag == cachedWar.ClanTag || c.Tag == cachedWar.OpponentTag).ToList();

                    if (string.IsNullOrWhiteSpace(cachedWar.WarTag) && _clansClient.UpdatingWar.TryAdd(cachedWar.Key, cachedWar.Content))
                    {
                        updatingWar.Add(cachedWar.Key);

                        tasks.Add(UpdateWarAsync(dbContext, cachedWar, cachedClans.FirstOrDefault(c => c.Tag == cachedWar.ClanTag), cachedClans.FirstOrDefault(c => c.Tag == cachedWar.OpponentTag)));
                    }

                    if (!string.IsNullOrWhiteSpace(cachedWar.WarTag) && _clansClient.UpdatingCwlWar.TryAdd(cachedWar.WarTag, cachedWar.Content))
                    {
                        updatingCwlWar.Add(cachedWar.WarTag);

                        tasks.Add(UpdateCwlWarAsync(cachedWar, cachedClans.FirstOrDefault(c => c.Tag == cachedWar.ClanTag), cachedClans.FirstOrDefault(c => c.Tag == cachedWar.OpponentTag)));
                    }

                    tasks.Add(SendWarAnnouncementsAsync(cachedWar, cachedClans.Select(c => c.CurrentWar).ToList()));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingWar)
                    _clansClient.UpdatingWar.TryRemove(tag, out _);

                foreach (string tag in updatingCwlWar)
                    _clansClient.UpdatingCwlWar.TryRemove(tag, out _);

                foreach (string tag in _unmonitoredClans)
                    _clansClient.UpdatingClan.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(Library.Monitors.Wars.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(Library.Monitors.Wars.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateCwlWarAsync(CachedWar cachedWar, CachedClan? cachedClan, CachedClan? cachedOpponent)
        {
            CachedWar? fetched = await CachedWar
                    .FromClanWarLeagueWarResponseAsync(cachedWar.WarTag, cachedWar.Season.Value, _clansClient, _clansApi, _cancellationToken)
                    .ConfigureAwait(false);

            if (cachedWar.Content != null &&
                fetched.Content != null &&
                cachedWar.Season == fetched.Season &&
                CachedWar.HasUpdated(cachedWar, fetched))
                await _clansClient.OnClanWarUpdatedAsync(new ClanWarUpdatedEventArgs(cachedWar.Content, fetched.Content, cachedClan?.Content, cachedOpponent?.Content, _cancellationToken)); 

            cachedWar.IsFinal = fetched.State == WarState.WarEnded;

            if (fetched.Content == null && !Clash.IsCwlEnabled)
                cachedWar.IsFinal = true;

            cachedWar.UpdateFrom(fetched);
        }

        private async Task UpdateWarAsync(CocApiCacheContext dbContext, CachedWar cachedWar, CachedClan? cachedClan, CachedClan? cachedOpponent)
        {
            if (cachedClan == null && cachedOpponent?.CurrentWar.IsExpired == true)
                cachedClan = await CreateCachedClan(cachedWar.ClanTag, dbContext).ConfigureAwait(false);

            if (cachedOpponent == null && cachedClan?.CurrentWar.IsExpired == true)
                cachedOpponent = await CreateCachedClan(cachedWar.OpponentTag, dbContext).ConfigureAwait(false);

            List<CachedClan?> cachedClans = new() { cachedClan, cachedOpponent };

            if (cachedClans.All(c => c?.CurrentWar.PreparationStartTime > cachedWar.PreparationStartTime) || cachedWar.EndTime.AddDays(8) < DateTime.UtcNow)
            {
                cachedWar.IsFinal = true;

                return;
            }

            CachedClan? clan = cachedClans
                .OrderByDescending(c => c?.CurrentWar.ExpiresAt)
                .FirstOrDefault(c => c?.CurrentWar.PreparationStartTime == cachedWar.PreparationStartTime);

            if (cachedWar.Content != null && clan?.CurrentWar.Content != null && CachedWar.HasUpdated(cachedWar, clan.CurrentWar))
                await _clansClient.OnClanWarUpdatedAsync(new ClanWarUpdatedEventArgs(cachedWar.Content, clan.CurrentWar.Content, cachedClan?.Content, cachedOpponent?.Content, _cancellationToken));

            if (clan != null)
            {
                cachedWar.UpdateFrom(clan.CurrentWar);

                cachedWar.IsFinal = clan.CurrentWar.State == WarState.WarEnded;
            }
        }

        private async Task<CachedClan?> CreateCachedClan(string tag, CocApiCacheContext dbContext)
        {
            if (!_clansClient.UpdatingClan.TryAdd(tag, null))
                return null;

            _unmonitoredClans.Add(tag);

            CachedClanWar cachedClanWar = await CachedClanWar.FromCurrentWarResponseAsync(tag, _clansClient, _clansApi, _cancellationToken).ConfigureAwait(false);

            cachedClanWar.Added = true;

            cachedClanWar.Download = false;

            CachedClan cachedClan = new(tag, false, false, false, false, false)
            {
                CurrentWar = cachedClanWar
            };

            await _semaphore.WaitAsync(_cancellationToken).ConfigureAwait(false);

            try
            {
                dbContext.Clans.Add(cachedClan);
            }
            finally
            {
                _semaphore.Release();
            }

            return cachedClan;
        }

        private async Task SendWarAnnouncementsAsync(CachedWar cachedWar, List<CachedClanWar> cachedClanWars)
        {
            try
            {
                if (cachedWar.Content == null)
                    return;

                if (cachedWar.Announcements.HasFlag(Announcements.WarStartingSoon) == false &&
                    DateTime.UtcNow > cachedWar.Content.StartTime.AddHours(-1) &&
                    DateTime.UtcNow < cachedWar.Content.StartTime)
                {
                    cachedWar.Announcements |= Announcements.WarStartingSoon;
                    await _clansClient.OnClanWarStartingSoonAsync(new WarEventArgs(cachedWar.Content, _cancellationToken));
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
                    DateTime.UtcNow > cachedWar.Content.EndTime.AddHours(-1) &&
                    DateTime.UtcNow < cachedWar.Content.EndTime)
                {
                    cachedWar.Announcements |= Announcements.WarEndingSoon;
                    await _clansClient.OnClanWarEndingSoonAsync(new WarEventArgs(cachedWar.Content, _cancellationToken));
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEndNotSeen) == false &&
                    cachedWar.State != WarState.WarEnded &&
                    DateTime.UtcNow > cachedWar.EndTime &&
                    DateTime.UtcNow < cachedWar.EndTime.AddHours(24) &&
                    cachedWar.Content.AllAttacksAreUsed() == false &&
                    cachedClanWars != null &&
                    cachedClanWars.All(w => w.Content != null && w.Content.PreparationStartTime != cachedWar.Content.PreparationStartTime))
                {
                    cachedWar.Announcements |= Announcements.WarEndNotSeen;
                    await _clansClient.OnClanWarEndNotSeenAsync(new WarEventArgs(cachedWar.Content, _cancellationToken));
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
                    cachedWar.EndTime < DateTime.UtcNow &&
                    cachedWar.EndTime.Day == DateTime.UtcNow.Day)
                {
                    cachedWar.Announcements |= Announcements.WarEnded;
                    await _clansClient.OnClanWarEndedAsync(new WarEventArgs(cachedWar.Content, _cancellationToken));
                }
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "SendWarAnnouncements error", e));

                //throw;
            }
        }
    }
}