using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache.Services
{
    public sealed class CwlWarService : PerpetualService<CwlWarService>
    {
        internal event AsyncEventHandler<WarEventArgs>? ClanWarEndingSoon;
        internal event AsyncEventHandler<WarEventArgs>? ClanWarEnded;
        internal event AsyncEventHandler<WarEventArgs>? ClanWarStartingSoon;
        internal event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;


        internal ClansApi ClansApi { get; }
        internal Synchronizer Synchronizer { get; }
        internal TimeToLiveProvider Ttl { get; }
        internal IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }


        public CwlWarService(
            CacheDbContextFactoryProvider provider, 
            ClansApi clansApi,
            Synchronizer synchronizer,
            TimeToLiveProvider ttl,
            IOptions<CacheOptions> options) 
        : base(provider, options.Value.CwlWars.DelayBeforeExecution, options.Value.CwlWars.DelayBetweenExecutions)
        {
            Instantiated = Library.EnsureSingleton(Instantiated);
            IsEnabled = options.Value.CwlWars.Enabled;
            ClansApi = clansApi;
            Synchronizer = synchronizer;
            Ttl = ttl;
            Options = options;
        }


        private protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            ServiceOptions options = Options.Value.CwlWars;

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<CachedWar> cachedWars = await dbContext.Wars
                .Where(w =>
                    !string.IsNullOrWhiteSpace(w.WarTag) &&
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

            List<Task> tasks = new();

            HashSet<string> updatingCwlWar = new();

            try
            {
                foreach (CachedWar cachedWar in cachedWars)
                {
                    if (Synchronizer.UpdatingCwlWar.TryAdd(cachedWar.WarTag, cachedWar.Content))
                    {
                        updatingCwlWar.Add(cachedWar.WarTag);

                        tasks.Add(UpdateCwlWarAsync(cachedWar, cancellationToken));
                    }

                    tasks.Add(SendWarAnnouncementsAsync(cachedWar, cancellationToken));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingCwlWar)
                    Synchronizer.UpdatingCwlWar.TryRemove(tag, out _);
            }
        }

        private async Task UpdateCwlWarAsync(CachedWar cachedWar, CancellationToken cancellationToken)
        {
            CachedWar? fetched = await CachedWar
                    .FromClanWarLeagueWarResponseAsync(cachedWar.WarTag, cachedWar.Season.Value, Ttl, ClansApi, cancellationToken)
                    .ConfigureAwait(false);

            if (cachedWar.Content != null &&
                fetched.Content != null &&
                cachedWar.Season == fetched.Season &&
                CachedWar.HasUpdated(cachedWar, fetched) &&
                ClanWarUpdated != null)
                await ClanWarUpdated
                    .Invoke(this, new ClanWarUpdatedEventArgs(cachedWar.Content, fetched.Content, null, null, cancellationToken))
                    .ConfigureAwait(false); 

            cachedWar.IsFinal = fetched.State == WarState.WarEnded;

            if (fetched.Content == null && !Clash.IsCwlEnabled)
                cachedWar.IsFinal = true;

            cachedWar.UpdateFrom(fetched);
        }

        private async Task SendWarAnnouncementsAsync(CachedWar cachedWar, CancellationToken cancellationToken)
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
                        await ClanWarStartingSoon
                            .Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken))
                            .ConfigureAwait(false);
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
                    now > cachedWar.Content.EndTime.AddHours(-1) &&
                    now < cachedWar.Content.EndTime)
                {
                    cachedWar.Announcements |= Announcements.WarEndingSoon;

                    if (ClanWarEndingSoon != null)
                        await ClanWarEndingSoon
                            .Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken))
                            .ConfigureAwait(false);
                }

                if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
                    cachedWar.EndTime < now &&
                    cachedWar.EndTime.Day <= (now.Day + 1))
                {
                    cachedWar.Announcements |= Announcements.WarEnded;

                    if (ClanWarEnded != null)
                        await ClanWarEnded
                            .Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken))
                            .ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                if (!cancellationToken.IsCancellationRequested)
                    await Library.OnLog(this, new LogEventArgs(LogLevel.Error, e, "SendWarAnnouncements error")).ConfigureAwait(false);

                //throw;
            }
        }
    }
}