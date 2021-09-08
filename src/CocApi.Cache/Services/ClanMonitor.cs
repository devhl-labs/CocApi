using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using CocApi.Cache.Services;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace CocApi.Cache
{
    public sealed class ClanMonitor : PerpetualMonitor<ClanMonitor>
    {
        private readonly ClansApi _clansApi;
        private readonly Synchronizer _synchronizer;
        private readonly TimeToLiveProvider _ttl;
        private readonly IOptions<ClanClientOptions> _options;
        public event AsyncEventHandler<ClanUpdatedEventArgs>? ClanUpdated;
        //public event AsyncEventHandler<WarAddedEventArgs>? ClanWarAdded;
        //public event AsyncEventHandler<WarEventArgs>? ClanWarEndingSoon;
        //public event AsyncEventHandler<WarEventArgs>? ClanWarEndNotSeen;
        //public event AsyncEventHandler<WarEventArgs>? ClanWarEnded;
        public event AsyncEventHandler<ClanWarLeagueGroupUpdatedEventArgs>? ClanWarLeagueGroupUpdated;
        public event AsyncEventHandler<ClanWarLogUpdatedEventArgs>? ClanWarLogUpdated;
        //public event AsyncEventHandler<WarEventArgs>? ClanWarStartingSoon;
        //public event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

        public ClanMonitor(
            CacheDbContextFactoryProvider provider, 
            ClansApi clansApi, 
            Synchronizer synchronizer,
            TimeToLiveProvider ttl,
            IOptions<ClanClientOptions> options) 
            : base(provider)
        {
            _clansApi = clansApi;
            _synchronizer = synchronizer;
            _ttl = ttl;
            _options = options;
        }

        //protected override async Task PollAsync()
//        {
//            MonitorOptions options = _options.Value.Clans;

//            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

//            List<CachedClan> cachedClans = await dbContext.Clans
//                .Where(c =>
//                    c.Id > _id &&
//                    (
//(!_options.Value.Clans.DisableClan       && (c.ExpiresAt ?? min)            < expires && (c.KeepUntil ?? min)            < now && c.Download) ||
//(!_options.Value.Clans.DisableGroup      && (c.Group.ExpiresAt ?? min)      < expires && (c.Group.KeepUntil ?? min)      < now && c.Group.Download) ||
//(!_options.Value.Clans.DisableCurrentWar && (c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < now && c.CurrentWar.Download && c.IsWarLogPublic != false) ||
//(!_options.Value.Clans.DisableWarLog     && (c.WarLog.ExpiresAt ?? min)     < expires && (c.WarLog.KeepUntil ?? min)     < now && c.WarLog.Download && c.IsWarLogPublic != false)
//                    ))
//                .OrderBy(c => c.Id)
//                .Take(options.ConcurrentUpdates)
//                .ToListAsync(cancellationToken)
//                .ConfigureAwait(false);

//            _id = cachedClans.Count == options.ConcurrentUpdates
//                ? cachedClans.Max(c => c.Id)
//                : int.MinValue;

//            List<Task> tasks = new();

//            HashSet<string> updatingTags = new();

//            try
//            {
//                foreach (CachedClan cachedClan in cachedClans)
//                {
//                    if (!_clansClient.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
//                        continue;

//                    updatingTags.Add(cachedClan.Tag);

//                    tasks.Add(TryUpdateAsync(cachedClan));
//                }

//                await Task.WhenAll(tasks);

//                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
//            }
//            finally
//            {
//                foreach (string tag in updatingTags)
//                    _clansClient.UpdatingClan.TryRemove(tag, out _);
//            }

//            if (_id == int.MinValue)
//                await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
//            else
//                await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
//        }

        private async Task TryUpdateAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            try
            {
                ExtendWarTTLWhileInCwl(cachedClan);

                List<Task> tasks = new();

                if (!_options.Value.Clans.DisableClan && cachedClan.Download && cachedClan.IsExpired)
                    tasks.Add(MonitorClanAsync(cachedClan, cancellationToken));

                if (!_options.Value.Clans.DisableCurrentWar && cachedClan.CurrentWar.Download && cachedClan.CurrentWar.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorClanWarAsync(cachedClan, cancellationToken));

                if (!_options.Value.Clans.DisableWarLog && cachedClan.WarLog.Download && cachedClan.WarLog.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorWarLogAsync(cachedClan, cancellationToken));

                if (!_options.Value.Clans.DisableGroup && cachedClan.Group.Download && cachedClan.Group.IsExpired)
                    tasks.Add(MonitorGroupAsync(cachedClan, cancellationToken));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        private async Task MonitorClanAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClan fetched = await CachedClan.FromClanResponseAsync(cachedClan.Tag, _ttl, _clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && ClanUpdated != null)
                await ClanUpdated
                    .Invoke(this, new ClanUpdatedEventArgs(cachedClan.Content, fetched.Content, cancellationToken))
                    .ConfigureAwait(false);

            cachedClan.UpdateFrom(fetched);
        }

        private async Task MonitorClanWarAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClanWar? fetched = await CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, _ttl, _clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }

        private async Task MonitorWarLogAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClanWarLog fetched = await CachedClanWarLog.FromClanWarLogResponseAsync(cachedClan.Tag, _ttl, _clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWarLog.HasUpdated(cachedClan.WarLog, fetched) && ClanWarLogUpdated != null)
                await ClanWarLogUpdated
                    .Invoke(this, new ClanWarLogUpdatedEventArgs(cachedClan.WarLog.Content, fetched.Content, cachedClan.Content, cancellationToken))
                    .ConfigureAwait(false);

            cachedClan.WarLog.UpdateFrom(fetched);
        }

        private async Task MonitorGroupAsync(CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedClanWarLeagueGroup? fetched = await CachedClanWarLeagueGroup
                .FromClanWarLeagueGroupResponseAsync(cachedClan.Tag, _ttl, _clansApi, cancellationToken)
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
                !_options.Value.CwlWars.Enabled ||
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

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            MonitorOptions options = _options.Value.Clans;

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    c.Id > _id &&
                    (
(!_options.Value.Clans.DisableClan && (c.ExpiresAt ?? min) < expires && (c.KeepUntil ?? min) < now && c.Download) ||
(!_options.Value.Clans.DisableGroup && (c.Group.ExpiresAt ?? min) < expires && (c.Group.KeepUntil ?? min) < now && c.Group.Download) ||
(!_options.Value.Clans.DisableCurrentWar && (c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < now && c.CurrentWar.Download && c.IsWarLogPublic != false) ||
(!_options.Value.Clans.DisableWarLog && (c.WarLog.ExpiresAt ?? min) < expires && (c.WarLog.KeepUntil ?? min) < now && c.WarLog.Download && c.IsWarLogPublic != false)
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
                    if (!_synchronizer.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
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
                    _synchronizer.UpdatingClan.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
        }
    }
}