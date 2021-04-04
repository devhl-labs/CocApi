using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    internal class WarMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;
        private readonly IOptions<ClientOptions> _options;
        private readonly HashSet<string> _unmonitoredClans = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public WarMonitor(CacheDbContextFactoryProvider provider, ClansApi clansApi, ClansClientBase clansClient, IOptions<ClientOptions> options) : base(provider)
        {
            _clansApi = clansApi;
            _clansClient = clansClient;
            _options = options;
        }

        protected override async Task PollAsync()
        {
            MonitorOptions options = _options.Value.Wars;

            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            List<CachedWar> cachedWars = await dbContext.Wars
                .Where(w =>
                    string.IsNullOrWhiteSpace(w.WarTag) &&
                    (w.ExpiresAt ?? min) < expires &&
                    (w.KeepUntil ?? min) < now &&
                    !w.IsFinal &&
                    w.Id > _id)
                .OrderBy(c => c.Id)
                .Take(options.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
                .ConfigureAwait(false);

            _id = cachedWars.Count == options.ConcurrentUpdates
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

            try
            {
                foreach (CachedWar cachedWar in cachedWars)
                {
                    List<CachedClan> cachedClans = allCachedClans.Where(c => c.Tag == cachedWar.ClanTag || c.Tag == cachedWar.OpponentTag).ToList();

                    updatingWar.Add(cachedWar.Key);

                    tasks.Add(UpdateWarAsync(dbContext, cachedWar, cachedClans.FirstOrDefault(c => c.Tag == cachedWar.ClanTag), cachedClans.FirstOrDefault(c => c.Tag == cachedWar.OpponentTag)));

                    tasks.Add(SendWarAnnouncementsAsync(cachedWar, cachedClans.Select(c => c.CurrentWar).ToList()));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingWar)
                    _clansClient.UpdatingWar.TryRemove(tag, out _);

                foreach (string tag in _unmonitoredClans)
                    _clansClient.UpdatingClan.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateWarAsync(CacheDbContext dbContext, CachedWar cachedWar, CachedClan? cachedClan, CachedClan? cachedOpponent)
        {
            if (cachedClan == null && cachedOpponent?.CurrentWar.IsExpired == true)
                cachedClan = await CreateCachedClan(cachedWar.ClanTag, dbContext).ConfigureAwait(false);

            if (cachedOpponent == null && cachedClan?.CurrentWar.IsExpired == true)
                cachedOpponent = await CreateCachedClan(cachedWar.OpponentTag, dbContext).ConfigureAwait(false);

            List<CachedClan?> cachedClans = new() { cachedClan, cachedOpponent };

            if (cachedClans.All(c => c?.CurrentWar.PreparationStartTime > cachedWar.PreparationStartTime) || cachedWar.EndTime.AddDays(8) < now)
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

        private async Task<CachedClan?> CreateCachedClan(string tag, CacheDbContext dbContext)
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
                    now > cachedWar.Content.StartTime.AddHours(-1) &&
                    now < cachedWar.Content.StartTime)
                {
                    cachedWar.Announcements |= Announcements.WarStartingSoon;
                    await _clansClient.OnClanWarStartingSoonAsync(new WarEventArgs(cachedWar.Content, _cancellationToken));
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
                    now > cachedWar.Content.EndTime.AddHours(-1) &&
                    now < cachedWar.Content.EndTime)
                {
                    cachedWar.Announcements |= Announcements.WarEndingSoon;
                    await _clansClient.OnClanWarEndingSoonAsync(new WarEventArgs(cachedWar.Content, _cancellationToken));
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEndNotSeen) == false &&
                    cachedWar.State != WarState.WarEnded &&
                    now > cachedWar.EndTime &&
                    now < cachedWar.EndTime.AddHours(24) &&
                    cachedWar.Content.AllAttacksAreUsed() == false &&
                    cachedClanWars != null &&
                    cachedClanWars.All(w => w.Content != null && w.Content.PreparationStartTime != cachedWar.Content.PreparationStartTime))
                {
                    cachedWar.Announcements |= Announcements.WarEndNotSeen;
                    await _clansClient.OnClanWarEndNotSeenAsync(new WarEventArgs(cachedWar.Content, _cancellationToken));
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
                    cachedWar.EndTime < now &&
                    cachedWar.EndTime.Day <= (now.Day + 1))
                {
                    cachedWar.Announcements |= Announcements.WarEnded;
                    await _clansClient.OnClanWarEndedAsync(new WarEventArgs(cachedWar.Content, _cancellationToken));
                }
            }
            catch (Exception e)
            {
                if (!_cancellationToken.IsCancellationRequested)
                    Library.OnLog(this, new LogEventArgs(LogLevel.Error, "SendWarAnnouncements error", e));

                //throw;
            }
        }
    }
}