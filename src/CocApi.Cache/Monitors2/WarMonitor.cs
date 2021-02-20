using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
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

        public async Task RunAsync()
        {
            try
            {
                if (_isRunning)
                    return;

                _isRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                Library.OnLog(this, new LogEventArgs(LogLevel.Information, "WarMonitor running"));

                while (_stopRequestedTokenSource.IsCancellationRequested == false)
                {
                    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

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
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    _id = cachedWars.Count == Library.Monitors.Wars.ConcurrentUpdates
                        ? cachedWars.Max(c => c.Id)
                        : int.MinValue;

                    HashSet<string> tags = new();

                    foreach(CachedWar cachedWar in cachedWars)
                    {
                        tags.Add(cachedWar.ClanTag);
                        tags.Add(cachedWar.OpponentTag);
                    }

                    List<CachedClan> allCachedClans = await dbContext.Clans
                        .Where(c => tags.Contains(c.Tag))
                        .AsNoTracking()
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    List<Task> tasks = new();

                    HashSet<string> updatingWar = new();

                    HashSet<string> updatingCwlWar = new();

                    try
                    {
                        foreach (CachedWar cachedWar in cachedWars)
                        {
                            List<CachedClan> cachedClans = allCachedClans.Where(c => c.Tag == cachedWar.ClanTag || c.Tag == cachedWar.OpponentTag).ToList();

                            if (string.IsNullOrWhiteSpace(cachedWar.WarTag))
                            {
                                if (!_clansClient.UpdatingWar.TryAdd(cachedWar.Key, cachedWar.Content))
                                    continue;
                                
                                updatingWar.Add(cachedWar.Key);

                                tasks.Add(UpdateWarAsync(dbContext, cachedWar, cachedClans.FirstOrDefault(c => c.Tag == cachedWar.ClanTag), cachedClans.FirstOrDefault(c => c.Tag == cachedWar.OpponentTag)));
                            }

                            if (!string.IsNullOrWhiteSpace(cachedWar.WarTag))
                            {
                                if (!_clansClient.UpdatingCwlWar.TryAdd(cachedWar.WarTag, cachedWar.Content))
                                    continue;

                                updatingCwlWar.Add(cachedWar.WarTag);

                                tasks.Add(UpdateCwlWarAsync(cachedWar, cachedClans.FirstOrDefault(c => c.Tag == cachedWar.ClanTag), cachedClans.FirstOrDefault(c => c.Tag == cachedWar.OpponentTag)));
                            }

                            await SendWarAnnouncementsAsync(cachedWar, cachedClans.Select(c => c.CurrentWar).ToList());
                        }

                        try
                        {
                            await Task.WhenAll(tasks).ConfigureAwait(false);
                        }
                        catch (Exception)
                        {
                        }

                        await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);
                    }
                    finally
                    {
                        foreach(string tag in updatingWar)
                            _clansClient.UpdatingWar.TryRemove(tag, out _);

                        foreach (string tag in updatingCwlWar)
                            _clansClient.UpdatingCwlWar.TryRemove(tag, out _);

                        foreach (string tag in _unmonitoredClans)
                            _clansClient.UpdatingClan.TryRemove(tag, out _);
                    }

                    if (_id == int.MinValue)
                        await Task.Delay(Library.Monitors.Wars.DelayBetweenBatches, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                    else
                        await Task.Delay(Library.Monitors.Wars.DelayBetweenBatchUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                }

                _isRunning = false;
            }
            catch (Exception e)
            {
                _isRunning = false;

                if (_stopRequestedTokenSource.IsCancellationRequested)
                    return;

                if (_stopRequestedTokenSource.IsCancellationRequested == false)
                {
                    Library.OnLog(this, new LogEventArgs(LogLevel.Error, "WarMonitor error", e));

                    _ = RunAsync();
                }
            }
        }

        private async Task UpdateCwlWarAsync(CachedWar cachedWar, CachedClan? cachedClan, CachedClan? cachedOpponent)
        {
            CachedWar? fetched = await CachedWar
                    .FromClanWarLeagueWarResponseAsync(cachedWar.WarTag, cachedWar.Season.Value, _clansClient, _clansApi, _stopRequestedTokenSource.Token)
                    .ConfigureAwait(false);

            if (cachedWar.Content != null &&
                fetched.Content != null &&
                cachedWar.Season == fetched.Season &&
                !_clansClient.HasUpdated(cachedWar, fetched))
                await _clansClient.OnClanWarUpdatedAsync(new ClanWarUpdatedEventArgs(cachedWar.Content, fetched.Content, cachedClan?.Content, cachedOpponent?.Content)); 

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

            List<CachedClan?> cachedClans = new List<CachedClan?> { cachedClan, cachedOpponent };

            if (cachedClans.All(c => c?.CurrentWar.PreparationStartTime > cachedWar.PreparationStartTime) || cachedWar.EndTime.AddDays(8) < DateTime.UtcNow)
            {
                cachedWar.IsFinal = true;

                return;
            }

            CachedClan? clan = cachedClans
                .OrderByDescending(c => c?.CurrentWar.ExpiresAt)
                .FirstOrDefault(c => c?.CurrentWar.PreparationStartTime == cachedWar.PreparationStartTime);

            if (cachedWar.Content != null && clan?.CurrentWar.Content != null && _clansClient.HasUpdated(cachedWar, clan.CurrentWar))
                await _clansClient.OnClanWarUpdatedAsync(new ClanWarUpdatedEventArgs(cachedWar.Content, clan.CurrentWar.Content, cachedClan?.Content, cachedOpponent?.Content));

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

            CachedClanWar cachedClanWar = await CachedClanWar.FromCurrentWarResponseAsync(tag, _clansClient, _clansApi, _stopRequestedTokenSource.Token).ConfigureAwait(false);

            cachedClanWar.Added = true;

            cachedClanWar.Download = false;

            CachedClan cachedClan = new CachedClan(tag, false, false, false, false, false)
            {
                CurrentWar = cachedClanWar
            };

            await _semaphore.WaitAsync().ConfigureAwait(false);

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

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken).ConfigureAwait(false);

            Library.OnLog(this, new LogEventArgs(LogLevel.Information, "WarMonitor stopped"));
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
                    await _clansClient.OnClanWarStartingSoonAsync(new WarEventArgs(cachedWar.Content));
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
                    DateTime.UtcNow > cachedWar.Content.EndTime.AddHours(-1) &&
                    DateTime.UtcNow < cachedWar.Content.EndTime)
                {
                    cachedWar.Announcements |= Announcements.WarEndingSoon;
                    await _clansClient.OnClanWarEndingSoonAsync(new WarEventArgs(cachedWar.Content));
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
                    await _clansClient.OnClanWarEndNotSeenAsync(new WarEventArgs(cachedWar.Content));
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
                    cachedWar.EndTime < DateTime.UtcNow &&
                    cachedWar.EndTime.Day == DateTime.UtcNow.Day)
                {
                    cachedWar.Announcements |= Announcements.WarEnded;
                    await _clansClient.OnClanWarEndedAsync(new WarEventArgs(cachedWar.Content));
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