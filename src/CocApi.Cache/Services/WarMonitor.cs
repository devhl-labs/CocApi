using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using CocApi.Cache.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public sealed class WarMonitor : PerpetualMonitor<WarMonitor>
    {
        public event AsyncEventHandler<WarEventArgs>? ClanWarEndingSoon;
        public event AsyncEventHandler<WarEventArgs>? ClanWarEndNotSeen;
        public event AsyncEventHandler<WarEventArgs>? ClanWarEnded;
        public event AsyncEventHandler<WarEventArgs>? ClanWarStartingSoon;
        public event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

        private readonly ClansApi _clansApi;
        private readonly IOptions<ClanClientOptions> _options;
        private readonly HashSet<string> _unmonitoredClans = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public static bool Instantiated { get; private set; }

        public Synchronizer Synchronizer { get; }

        public TimeToLiveProvider TimeToLiveProvider { get; }

        public WarMonitor(
            CacheDbContextFactoryProvider provider,
            ClansApi clansApi,
            Synchronizer synchronizer,
            TimeToLiveProvider timeToLiveProvider,
            IOptions<ClanClientOptions> options) 
        : base(provider)
        {
            Instantiated = Library.EnsureSingleton(Instantiated);
            _clansApi = clansApi;
            //_clansClient = clansClient;
            Synchronizer = synchronizer;
            TimeToLiveProvider = timeToLiveProvider;
            _options = options;
        }

        private async Task UpdateWarAsync(
            CacheDbContext dbContext, 
            CachedWar cachedWar, 
            CachedClan? cachedClan, 
            CachedClan? cachedOpponent, 
            CancellationToken cancellationToken)
        {
            if (cachedClan == null && cachedOpponent?.CurrentWar.IsExpired == true)
                cachedClan = await CreateCachedClan(cachedWar.ClanTag, dbContext, cancellationToken).ConfigureAwait(false);

            if (cachedOpponent == null && cachedClan?.CurrentWar.IsExpired == true)
                cachedOpponent = await CreateCachedClan(cachedWar.OpponentTag, dbContext, cancellationToken).ConfigureAwait(false);

            List<CachedClan?> cachedClans = new() { cachedClan, cachedOpponent };

            if (cachedClans.All(c => c?.CurrentWar.PreparationStartTime > cachedWar.PreparationStartTime) || cachedWar.EndTime.AddDays(8) < now)
            {
                cachedWar.IsFinal = true;

                return;
            }

            CachedClan? clan = cachedClans
                .OrderByDescending(c => c?.CurrentWar.ExpiresAt)
                .FirstOrDefault(c => c?.CurrentWar.PreparationStartTime == cachedWar.PreparationStartTime);

            if (cachedWar.Content != null && 
                clan?.CurrentWar.Content != null && 
                CachedWar.HasUpdated(cachedWar, clan.CurrentWar) &&
                ClanWarUpdated != null)
                await ClanWarUpdated.Invoke(this, new ClanWarUpdatedEventArgs(
                        cachedWar.Content, 
                        clan.CurrentWar.Content, 
                        cachedClan?.Content, 
                        cachedOpponent?.Content, 
                        cancellationToken))
                    .ConfigureAwait(false);

            if (clan != null)
            {
                cachedWar.UpdateFrom(clan.CurrentWar);

                cachedWar.IsFinal = clan.CurrentWar.State == WarState.WarEnded;
            }
        }

        private async Task<CachedClan?> CreateCachedClan(string tag, CacheDbContext dbContext, CancellationToken cancellationToken)
        {
            if (!Synchronizer.UpdatingClan.TryAdd(tag, null))
                return null;

            try
            {
                _unmonitoredClans.Add(tag);

                CachedClanWar cachedClanWar = await CachedClanWar
                    .FromCurrentWarResponseAsync(tag, TimeToLiveProvider, _clansApi, cancellationToken)
                    .ConfigureAwait(false);

                cachedClanWar.Added = true;

                cachedClanWar.Download = false;

                CachedClan cachedClan = new(tag, false, false, false, false, false)
                {
                    CurrentWar = cachedClanWar
                };

                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

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
            finally
            {
                Synchronizer.UpdatingClan.TryRemove(tag, out _);
            }
        }

        private async Task SendWarAnnouncementsAsync(CachedWar cachedWar, CachedClanWar[] cachedClanWars, CancellationToken cancellationToken)
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

                    if (ClanWarStartingSoon != null)
                        await ClanWarStartingSoon.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
                    now > cachedWar.Content.EndTime.AddHours(-1) &&
                    now < cachedWar.Content.EndTime)
                {
                    cachedWar.Announcements |= Announcements.WarEndingSoon;

                    if (ClanWarEndingSoon != null)
                        await ClanWarEndingSoon.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
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

                    if (ClanWarEndNotSeen != null)
                        await ClanWarEndNotSeen.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
                    cachedWar.EndTime < now &&
                    cachedWar.EndTime.Day <= (now.Day + 1))
                {
                    cachedWar.Announcements |= Announcements.WarEnded;

                    if (ClanWarEnded != null)
                        await ClanWarEnded.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                if (!cancellationToken.IsCancellationRequested)
                    await Library.OnLog(this, new LogEventArgs(LogLevel.Error, e, "SendWarAnnouncements error")).ConfigureAwait(false);
            }
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            MonitorOptions options = _options.Value.Wars;

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<CachedWar> cachedWars = await dbContext.Wars
                .Where(w =>
                    string.IsNullOrWhiteSpace(w.WarTag) &&
                    (w.ExpiresAt ?? min) < expires &&
                    (w.KeepUntil ?? min) < now &&
                    !w.IsFinal &&
                    w.Id > _id)
                .OrderBy(c => c.Id)
                .Take(options.ConcurrentUpdates)
                .ToListAsync(cancellationToken)
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
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            List<Task> tasks = new();

            HashSet<string> updatingWar = new();

            try
            {
                foreach (CachedWar cachedWar in cachedWars)
                {
                    List<CachedClan> cachedClans = allCachedClans.Where(c => c.Tag == cachedWar.ClanTag || c.Tag == cachedWar.OpponentTag).ToList();

                    updatingWar.Add(cachedWar.Key);

                    tasks.Add(
                        UpdateWarAsync(
                            dbContext, 
                            cachedWar, 
                            cachedClans.FirstOrDefault(c => c.Tag == cachedWar.ClanTag), 
                            cachedClans.FirstOrDefault(c => c.Tag == cachedWar.OpponentTag), 
                            cancellationToken));

                    tasks.Add(SendWarAnnouncementsAsync(cachedWar, cachedClans.Select(c => c.CurrentWar).ToArray(), cancellationToken));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingWar)
                    Synchronizer.UpdatingWar.TryRemove(tag, out _);

                foreach (string tag in _unmonitoredClans)
                    Synchronizer.UpdatingClan.TryRemove(tag, out _);
            }

            // todo what am i doing with this?
            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
        }
    }
}