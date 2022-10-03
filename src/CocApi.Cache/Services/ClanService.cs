﻿using System;
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
using CocApi.Cache.Services.Options;

namespace CocApi.Cache.Services
{
    public sealed class ClanService : ServiceBase
    {
        internal event AsyncEventHandler<ClanUpdatedEventArgs>? ClanUpdated;
        internal event AsyncEventHandler<ClanWarLeagueGroupUpdatedEventArgs>? ClanWarLeagueGroupUpdated;
        internal event AsyncEventHandler<ClanWarLogUpdatedEventArgs>? ClanWarLogUpdated;


        internal IApiFactory ApiFactory { get; }
        internal Synchronizer Synchronizer { get; }
        internal TimeToLiveProvider Ttl { get; }
        internal IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }


        public ClanService(
            ILogger<ClanService> logger,
            IServiceScopeFactory scopeFactory,
            IApiFactory apiFactory,
            Synchronizer synchronizer,
            TimeToLiveProvider ttl,
            IOptions<CacheOptions> options)
            : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.Clans))
        {
            Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
            ApiFactory = apiFactory;
            Synchronizer = synchronizer;
            Ttl = ttl;
            Options = options;
        }


        protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            ClanServiceOptions options = Options.Value.Clans;

            using var scope = ScopeFactory.CreateScope();

            CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    c.Id > _id &&
                    (
(options.DownloadClan && (c.ExpiresAt ?? min) < expires && (c.KeepUntil ?? min) < now && c.Download) ||
(options.DownloadGroup && (c.Group.ExpiresAt ?? min) < expires && (c.Group.KeepUntil ?? min) < now && c.Group.Download) ||
(options.DownloadCurrentWar && (c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < now && c.CurrentWar.Download && c.IsWarLogPublic != false) ||
(options.DownloadWarLog && (c.WarLog.ExpiresAt ?? min) < expires && (c.WarLog.KeepUntil ?? min) < now && c.WarLog.Download && c.IsWarLogPublic != false)
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

                IClansApi clansApi = ApiFactory.Create<IClansApi>();

                ClanServiceOptions options = Options.Value.Clans;

                if (options.DownloadClan && cachedClan.Download && cachedClan.IsExpired)
                    tasks.Add(MonitorClanAsync(clansApi, cachedClan, cancellationToken));

                if (options.DownloadCurrentWar && cachedClan.CurrentWar.Download && cachedClan.CurrentWar.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorClanWarAsync(clansApi, cachedClan, cancellationToken));

                if (options.DownloadWarLog && cachedClan.WarLog.Download && cachedClan.WarLog.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorWarLogAsync(clansApi, cachedClan, cancellationToken));

                if (options.DownloadGroup && cachedClan.Group.Download && cachedClan.Group.IsExpired)
                    tasks.Add(MonitorGroupAsync(clansApi, cachedClan, cancellationToken));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        private async Task MonitorClanAsync(IClansApi clansApi, CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClan fetched = await CachedClan.FromClanResponseAsync(cachedClan.Tag, Ttl, clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && ClanUpdated != null && CachedClan.HasUpdated(cachedClan, fetched))
                await ClanUpdated
                    .Invoke(this, new ClanUpdatedEventArgs(cachedClan.Content, fetched.Content, cancellationToken))
                    .ConfigureAwait(false);

            cachedClan.UpdateFrom(fetched);
        }

        private async Task MonitorClanWarAsync(IClansApi clansApi, CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClanWar fetched = await CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, Ttl, clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }

        private async Task MonitorWarLogAsync(IClansApi clansApi, CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClanWarLog fetched = await CachedClanWarLog.FromClanWarLogResponseAsync(cachedClan.Tag, Ttl, clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWarLog.HasUpdated(cachedClan.WarLog, fetched) && ClanWarLogUpdated != null)
                await ClanWarLogUpdated
                    .Invoke(this, new ClanWarLogUpdatedEventArgs(cachedClan.WarLog.Content, fetched.Content, cachedClan.Content, cancellationToken))
                    .ConfigureAwait(false);

            cachedClan.WarLog.UpdateFrom(fetched);
        }

        private async Task MonitorGroupAsync(IClansApi clansApi, CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClanWarLeagueGroup fetched = await CachedClanWarLeagueGroup
                .FromClanWarLeagueGroupResponseAsync(cachedClan.Tag, Ttl, clansApi, cancellationToken)
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
                cachedClan.CurrentWar.Content?.State == Rest.Models.WarState.InWar ||
                cachedClan.CurrentWar.Content?.State == Rest.Models.WarState.Preparation ||
                cachedClan.Group.Content == null ||
                cachedClan.Group.Content.State == Rest.Models.GroupState.Ended ||
                cachedClan.Group.Content.Season.Month < DateTime.UtcNow.Month ||
                (cachedClan.Group.KeepUntil.HasValue && cachedClan.Group.KeepUntil.Value.Month > DateTime.UtcNow.Month))
                return;

            // keep currentwar around an arbitrary amount of time since we are in cwl
            cachedClan.CurrentWar.KeepUntil = DateTime.UtcNow.AddMinutes(20);
        }
    }
}