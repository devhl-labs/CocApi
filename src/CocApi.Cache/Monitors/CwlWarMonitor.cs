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
    internal class CwlWarMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;
        private readonly IOptions<ClanClientOptions> _options;

        public CwlWarMonitor(CacheDbContextFactoryProvider provider, ClansApi clansApi, ClansClientBase clansClient, IOptions<ClanClientOptions> options) : base(provider)
        {
            _clansApi = clansApi;
            _clansClient = clansClient;
            _options = options;
        }

        protected override async Task PollAsync()
        {
            MonitorOptions options = _options.Value.CwlWars;

            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            List<CachedWar> cachedWars = await dbContext.Wars
                .Where(w =>
                    !string.IsNullOrWhiteSpace(w.WarTag) &&
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

            List<Task> tasks = new();

            HashSet<string> updatingCwlWar = new();

            try
            {
                foreach (CachedWar cachedWar in cachedWars)
                {
                    if (_clansClient.UpdatingCwlWar.TryAdd(cachedWar.WarTag, cachedWar.Content))
                    {
                        updatingCwlWar.Add(cachedWar.WarTag);

                        tasks.Add(UpdateCwlWarAsync(cachedWar));
                    }

                    tasks.Add(SendWarAnnouncementsAsync(cachedWar));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingCwlWar)
                    _clansClient.UpdatingCwlWar.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateCwlWarAsync(CachedWar cachedWar)
        {
            CachedWar? fetched = await CachedWar
                    .FromClanWarLeagueWarResponseAsync(cachedWar.WarTag, cachedWar.Season.Value, _clansClient, _clansApi, _cancellationToken)
                    .ConfigureAwait(false);

            if (cachedWar.Content != null &&
                fetched.Content != null &&
                cachedWar.Season == fetched.Season &&
                CachedWar.HasUpdated(cachedWar, fetched))
                await _clansClient.OnClanWarUpdatedAsync(new ClanWarUpdatedEventArgs(cachedWar.Content, fetched.Content, null, null, _cancellationToken)); 

            cachedWar.IsFinal = fetched.State == WarState.WarEnded;

            if (fetched.Content == null && !Clash.IsCwlEnabled)
                cachedWar.IsFinal = true;

            cachedWar.UpdateFrom(fetched);
        }

        private async Task SendWarAnnouncementsAsync(CachedWar cachedWar)
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

                //if (cachedWar.Announcements.HasFlag(Announcements.WarEndNotSeen) == false &&
                //    cachedWar.State != WarState.WarEnded &&
                //    now > cachedWar.EndTime &&
                //    now < cachedWar.EndTime.AddHours(24) &&
                //    cachedWar.Content.AllAttacksAreUsed() == false &&
                //    cachedClanWars != null &&
                //    cachedClanWars.All(w => w.Content != null && w.Content.PreparationStartTime != cachedWar.Content.PreparationStartTime))
                //{
                //    cachedWar.Announcements |= Announcements.WarEndNotSeen;
                //    await _clansClient.OnClanWarEndNotSeenAsync(new WarEventArgs(cachedWar.Content, _cancellationToken));
                //}

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
                    Library.OnLog(this, new LogEventArgs(LogLevel.Error, e, "SendWarAnnouncements error"));

                //throw;
            }
        }
    }
}