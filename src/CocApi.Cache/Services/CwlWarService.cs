using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.IApis;
using CocApi.Cache.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CocApi.Rest.Client;
using CocApi.Cache.Options;

namespace CocApi.Cache.Services
{
    public sealed class CwlWarService : PerpetualService
    {
        internal event AsyncEventHandler<WarEventArgs>? ClanWarEndingSoon;
        internal event AsyncEventHandler<WarEventArgs>? ClanWarEnded;
        internal event AsyncEventHandler<WarEventArgs>? ClanWarStartingSoon;
        internal event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

        public ILogger<CwlWarService> Logger { get; }

        internal IApiFactory ApiFactory { get; }
        internal Synchronizer Synchronizer { get; }
        internal TimeToLiveProvider Ttl { get; }
        public IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }


        public CwlWarService(
            ILogger<CwlWarService> logger,
            IServiceScopeFactory scopeFactory,
            IApiFactory apiFactory,
            Synchronizer synchronizer,
            TimeToLiveProvider ttl,
            IOptions<CacheOptions> options)
        : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.CwlWars))
        {
            Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
            Logger = logger;
            ApiFactory = apiFactory;
            Synchronizer = synchronizer;
            Ttl = ttl;
            Options = options;
        }


        protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            CwlWarServiceOptions options = Options.Value.CwlWars;

            using var scope = ScopeFactory.CreateScope();

            CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

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
                IClansApi clansApi = ApiFactory.Create<IClansApi>();

                foreach (CachedWar cachedWar in cachedWars)
                {
                    if (Synchronizer.UpdatingCwlWar.TryAdd(cachedWar.WarTag, cachedWar.Content))
                    {
                        updatingCwlWar.Add(cachedWar.WarTag);

                        tasks.Add(UpdateCwlWarAsync(clansApi, cachedWar, cancellationToken));
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

        private async Task UpdateCwlWarAsync(IClansApi clansApi, CachedWar cachedWar, CancellationToken cancellationToken)
        {
            CachedWar? fetched = await CachedWar
                    .FromClanWarLeagueWarResponseAsync(cachedWar.WarTag, cachedWar.Season.Value, Ttl, clansApi, cancellationToken)
                    .ConfigureAwait(false);

            if (cachedWar.Content != null &&
                fetched.Content != null &&
                cachedWar.Season == fetched.Season &&
                CachedWar.HasUpdated(cachedWar, fetched) &&
                ClanWarUpdated != null)
                await ClanWarUpdated
                    .Invoke(this, new ClanWarUpdatedEventArgs(cachedWar.Content, fetched.Content, null, null, cancellationToken))
                    .ConfigureAwait(false); 

            cachedWar.IsFinal = (fetched.Content == null && !Clash.IsCwlEnabled) || fetched.State == Rest.Models.WarState.WarEnded;

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
                    Logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", GetType().Name, nameof(SendWarAnnouncementsAsync));

                //throw;
            }
        }
    }
}