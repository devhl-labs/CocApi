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
    internal class ClanMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;
        private readonly IOptions<ClanClientOptions> _options;

        public ClanMonitor(CacheDbContextFactoryProvider provider, ClansApi clansApi, ClansClientBase clansClient, IOptions<ClanClientOptions> options) : base(provider)
        {
            _clansApi = clansApi;
            _clansClient = clansClient;
            _options = options;
        }

        protected override async Task PollAsync()
        {
            MonitorOptions options = _options.Value.Clans;

            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    c.Id > _id &&
                    (
(!_options.Value.Clans.DisableClan       && (c.ExpiresAt ?? min)            < expires && (c.KeepUntil ?? min)            < now && c.Download) ||
(!_options.Value.Clans.DisableGroup      && (c.Group.ExpiresAt ?? min)      < expires && (c.Group.KeepUntil ?? min)      < now && c.Group.Download) ||
(!_options.Value.Clans.DisableCurrentWar && (c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < now && c.CurrentWar.Download && c.IsWarLogPublic != false) ||
(!_options.Value.Clans.DisableWarLog     && (c.WarLog.ExpiresAt ?? min)     < expires && (c.WarLog.KeepUntil ?? min)     < now && c.WarLog.Download && c.IsWarLogPublic != false)
                    ))
                .OrderBy(c => c.Id)
                .Take(options.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
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
                    if (!_clansClient.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                        continue;

                    updatingTags.Add(cachedClan.Tag);

                    tasks.Add(TryUpdateAsync(cachedClan));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingTags)
                    _clansClient.UpdatingClan.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task TryUpdateAsync(CachedClan cachedClan)
        {
            try
            {
                ExtendWarTTLWhileInCwl(cachedClan);

                List<Task> tasks = new();

                if (!_options.Value.Clans.DisableClan && cachedClan.Download && cachedClan.IsExpired)
                    tasks.Add(MonitorClanAsync(cachedClan));

                if (!_options.Value.Clans.DisableCurrentWar && cachedClan.CurrentWar.Download && cachedClan.CurrentWar.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorClanWarAsync(cachedClan));

                if (!_options.Value.Clans.DisableWarLog && cachedClan.WarLog.Download && cachedClan.WarLog.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorWarLogAsync(cachedClan));

                if (!_options.Value.Clans.DisableGroup && cachedClan.Group.Download && cachedClan.Group.IsExpired)
                    tasks.Add(MonitorGroupAsync(cachedClan));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        private async Task MonitorClanAsync(CachedClan cachedClan)
        {
            CachedClan fetched = await CachedClan.FromClanResponseAsync(cachedClan.Tag, _clansClient, _clansApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && _clansClient.HasUpdatedOrDefault(cachedClan.Content, fetched.Content)) // CachedClan.HasUpdated(cachedClan, fetched))
                await _clansClient.OnClanUpdatedAsync(new ClanUpdatedEventArgs(cachedClan.Content, fetched.Content, _cancellationToken));

            cachedClan.UpdateFrom(fetched);
        }

        private async Task MonitorClanWarAsync(CachedClan cachedClan)
        {
            CachedClanWar? fetched = await CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, _clansClient, _clansApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }

        private async Task MonitorWarLogAsync(CachedClan cachedClan)
        {
            CachedClanWarLog fetched = await CachedClanWarLog.FromClanWarLogResponseAsync(cachedClan.Tag, _clansClient, _clansApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWarLog.HasUpdated(cachedClan.WarLog, fetched))
                await _clansClient.OnClanWarLogUpdatedAsync(new ClanWarLogUpdatedEventArgs(cachedClan.WarLog.Content, fetched.Content, cachedClan.Content, _cancellationToken));

            cachedClan.WarLog.UpdateFrom(fetched);
        }

        private async Task MonitorGroupAsync(CachedClan cachedClan)
        {
            CachedClanWarLeagueGroup? fetched = await CachedClanWarLeagueGroup
                .FromClanWarLeagueGroupResponseAsync(cachedClan.Tag, _clansClient, _clansApi, _cancellationToken)
                .ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWarLeagueGroup.HasUpdated(cachedClan.Group, fetched))
            {
                await _clansClient.OnClanWarLeagueGroupUpdatedAsync(new ClanWarLeagueGroupUpdatedEventArgs(cachedClan.Group.Content, fetched.Content, cachedClan.Content, _cancellationToken));

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
    }
}