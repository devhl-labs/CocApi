using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Cache.Services
{
    public sealed class ClanService : PerpetualService<ClanService>
    {
        internal event AsyncEventHandler<ClanUpdatedEventArgs>? ClanUpdated;
        internal event AsyncEventHandler<ClanWarLeagueGroupUpdatedEventArgs>? ClanWarLeagueGroupUpdated;
        internal event AsyncEventHandler<ClanWarLogUpdatedEventArgs>? ClanWarLogUpdated;


        internal ClansApi ClansApi { get; }
        internal Synchronizer Synchronizer { get; }
        internal TimeToLiveProvider Ttl { get; }
        internal IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }


        public ClanService(
            ILogger<ClanService> logger,
            CacheDbContextFactoryProvider provider, 
            ClansApi clansApi, 
            Synchronizer synchronizer,
            TimeToLiveProvider ttl,
            IOptions<CacheOptions> options) 
            : base(logger, provider, options.Value.Clans.DelayBeforeExecution, options.Value.Clans.DelayBetweenExecutions)
        {
            Instantiated = Library.EnsureSingleton(Instantiated);
            IsEnabled = options.Value.Clans.Enabled;
            ClansApi = clansApi;
            Synchronizer = synchronizer;
            Ttl = ttl;
            Options = options;
        }


        private protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            ServiceOptions options = Options.Value.Clans;

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    c.Id > _id &&
                    (
(Options.Value.Clans.DownloadClan && (c.ExpiresAt ?? min) < expires && (c.KeepUntil ?? min) < now && c.Download) ||
(Options.Value.Clans.DownloadGroup && (c.Group.ExpiresAt ?? min) < expires && (c.Group.KeepUntil ?? min) < now && c.Group.Download) ||
(Options.Value.Clans.DownloadCurrentWar && (c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < now && c.CurrentWar.Download && c.IsWarLogPublic != false) ||
(Options.Value.Clans.DownloadWarLog && (c.WarLog.ExpiresAt ?? min) < expires && (c.WarLog.KeepUntil ?? min) < now && c.WarLog.Download && c.IsWarLogPublic != false)
                    ))
                .OrderBy(c => c.Id)
                .Take(options.ConcurrentUpdates)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            _id = cachedClans.Count == options.ConcurrentUpdates
                ? cachedClans.Max(c => c.Id)
                : int.MinValue;

            List<Task> tasks = new();

            HashSet<string> updatingTags = new();

            try
            {
                foreach (CachedClan cachedClan in cachedClans)
                {
                    if (!Synchronizer.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                        continue;

                    updatingTags.Add(cachedClan.Tag);

                    tasks.Add(TryUpdateAsync(cachedClan, cancellationToken));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingTags)
                    Synchronizer.UpdatingClan.TryRemove(tag, out _);
            }
        }

        private async Task TryUpdateAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            try
            {
                ExtendWarTTLWhileInCwl(cachedClan);

                List<Task> tasks = new();

                if (Options.Value.Clans.DownloadClan && cachedClan.Download && cachedClan.IsExpired)
                    tasks.Add(MonitorClanAsync(cachedClan, cancellationToken));

                if (Options.Value.Clans.DownloadCurrentWar && cachedClan.CurrentWar.Download && cachedClan.CurrentWar.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorClanWarAsync(cachedClan, cancellationToken));

                if (Options.Value.Clans.DownloadWarLog && cachedClan.WarLog.Download && cachedClan.WarLog.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorWarLogAsync(cachedClan, cancellationToken));

                if (Options.Value.Clans.DownloadGroup && cachedClan.Group.Download && cachedClan.Group.IsExpired)
                    tasks.Add(MonitorGroupAsync(cachedClan, cancellationToken));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        private async Task MonitorClanAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClan fetched = await CachedClan.FromClanResponseAsync(cachedClan.Tag, Ttl, ClansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && ClanUpdated != null && CachedClan.HasUpdated(cachedClan, fetched))
                await ClanUpdated
                    .Invoke(this, new ClanUpdatedEventArgs(cachedClan.Content, fetched.Content, cancellationToken))
                    .ConfigureAwait(false);

            cachedClan.UpdateFrom(fetched);
        }

        private async Task MonitorClanWarAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClanWar? fetched = await CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, Ttl, ClansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }

        private async Task MonitorWarLogAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClanWarLog fetched = await CachedClanWarLog.FromClanWarLogResponseAsync(cachedClan.Tag, Ttl, ClansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWarLog.HasUpdated(cachedClan.WarLog, fetched) && ClanWarLogUpdated != null)
                await ClanWarLogUpdated
                    .Invoke(this, new ClanWarLogUpdatedEventArgs(cachedClan.WarLog.Content, fetched.Content, cachedClan.Content, cancellationToken))
                    .ConfigureAwait(false);

            cachedClan.WarLog.UpdateFrom(fetched);
        }

        private async Task MonitorGroupAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClanWarLeagueGroup? fetched = await CachedClanWarLeagueGroup
                .FromClanWarLeagueGroupResponseAsync(cachedClan.Tag, Ttl, ClansApi, cancellationToken)
                .ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWarLeagueGroup.HasUpdated(cachedClan.Group, fetched))
            {
                if (ClanWarLeagueGroupUpdated != null)
                    await ClanWarLeagueGroupUpdated
                        .Invoke(this, new ClanWarLeagueGroupUpdatedEventArgs(cachedClan.Group.Content, fetched.Content, cachedClan.Content, cancellationToken))
                        .ConfigureAwait(false);

                cachedClan.Group.Added = false;                
            }

            cachedClan.Group.UpdateFrom(fetched);
        }

        private void ExtendWarTTLWhileInCwl(CachedClan cachedClan)
        {
            if (!Clash.IsCwlEnabled ||
                !Options.Value.CwlWars.Enabled ||
                cachedClan.CurrentWar.Content?.State == WarState.InWar ||
                cachedClan.CurrentWar.Content?.State == WarState.Preparation ||
                cachedClan.Group.Content == null ||
                cachedClan.Group.Content.State == GroupState.Ended ||
                cachedClan.Group.Content.Season.Month < DateTime.UtcNow.Month ||
                (cachedClan.Group.KeepUntil.HasValue && cachedClan.Group.KeepUntil.Value.Month > DateTime.UtcNow.Month))
                return;

            // keep currentwar around an arbitrary amount of time since we are in cwl
            cachedClan.CurrentWar.KeepUntil = DateTime.UtcNow.AddMinutes(20);
        }
    }
}