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
    internal class ClanMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        public ClanMonitor(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs, ClansApi clansApi, ClansClientBase clansClientBase)
            : base(dbContextFactory, dbContextArgs)
        {
            _clansApi = clansApi;
            _clansClient = clansClientBase;
        }

        protected override async Task PollAsync()
        {
            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            DateTime expires = DateTime.UtcNow.AddSeconds(-3);

            DateTime min = DateTime.MinValue;

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    (
((c.ExpiresAt ?? min) < expires && (c.KeepUntil ?? min) < DateTime.UtcNow && c.Download) ||
((c.Group.ExpiresAt ?? min) < expires && (c.Group.KeepUntil ?? min) < DateTime.UtcNow && c.Group.Download) ||
((c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < DateTime.UtcNow && c.CurrentWar.Download && c.IsWarLogPublic != false) ||
((c.WarLog.ExpiresAt ?? min) < expires && (c.WarLog.KeepUntil ?? min) < DateTime.UtcNow && c.WarLog.Download && c.IsWarLogPublic != false)
                    )
                    &&
                    c.Id > _id)
                .OrderBy(c => c.Id)
                .Take(Library.Monitors.Clans.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
                .ConfigureAwait(false);

            _id = cachedClans.Count == Library.Monitors.Clans.ConcurrentUpdates
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
                await Task.Delay(Library.Monitors.Clans.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(Library.Monitors.Clans.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task TryUpdateAsync(CachedClan cachedClan)
        {
            try
            {
                List<Task> tasks = new();

                if (cachedClan.Download && cachedClan.IsExpired)
                    tasks.Add(MonitorClanAsync(cachedClan));

                if (cachedClan.CurrentWar.Download && cachedClan.CurrentWar.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorClanWarAsync(cachedClan));

                if (cachedClan.WarLog.Download && cachedClan.WarLog.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                    tasks.Add(MonitorWarLogAsync(cachedClan));

                if (cachedClan.Group.Download && cachedClan.Group.IsExpired)
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

            if (fetched.Content != null && CachedClan.HasUpdated(cachedClan, fetched))
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
    }
}