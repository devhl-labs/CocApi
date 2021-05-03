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
    internal class PlayerMonitor : MonitorBase
    {
        private readonly PlayersApi _playersApi;
        private readonly PlayersClientBase _playersClientBase;
        private readonly IOptions<MonitorOptions> _options;
        private DateTime _deletedUnmonitoredPlayers = DateTime.UtcNow;

        public PlayerMonitor(CacheDbContextFactoryProvider provider, PlayersApi playersApi, PlayersClientBase playersClient, IOptions<MonitorOptions> options) : base(provider)
        {
            _playersApi = playersApi;
            _playersClientBase = playersClient;
            _options = options;
        }

        protected override async Task PollAsync()
        {
            MonitorOptions options = _options.Value;

            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            List<Context.CachedItems.CachedPlayer> trackedPlayers = await dbContext.Players
                .Where(p =>
                    (p.ExpiresAt ?? min) < expires &&
                    (p.KeepUntil ?? min) < now &&
                    p.Download &&
                    p.Id > _id)
                .OrderBy(p => p.Id)
                .Take(options.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
                .ConfigureAwait(false);

            _id = trackedPlayers.Count == options.ConcurrentUpdates
                ? trackedPlayers.Max(c => c.Id)
                : int.MinValue;

            List<Task> tasks = new();

            HashSet<string> updatingTags = new();

            try
            {
                foreach (Context.CachedItems.CachedPlayer trackedPlayer in trackedPlayers)
                {
                    if (!_playersClientBase.UpdatingVillage.TryAdd(trackedPlayer.Tag, trackedPlayer))
                        continue;

                    updatingTags.Add(trackedPlayer.Tag);

                    if (trackedPlayer.Download && trackedPlayer.IsExpired)
                        tasks.Add(MonitorPlayerAsync(trackedPlayer));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingTags)
                    _playersClientBase.UpdatingVillage.TryRemove(tag, out _);
            }

            if (_deletedUnmonitoredPlayers < DateTime.UtcNow.AddMinutes(-10))
            {
                _deletedUnmonitoredPlayers = DateTime.UtcNow;

                await DeletePlayersNotMonitoredAsync(dbContext).ConfigureAwait(false);
            }

            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task MonitorPlayerAsync(CachedPlayer cachedPlayer)
        {
            CachedPlayer fetched = await CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, _playersClientBase, _playersApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && _playersClientBase.HasUpdatedOrDefault(fetched.Content, cachedPlayer.Content)) // CachedPlayer.HasUpdated(cachedPlayer, fetched))
                await _playersClientBase.OnPlayerUpdatedAsync(new PlayerUpdatedEventArgs(cachedPlayer.Content, fetched.Content, _cancellationToken));

            cachedPlayer.UpdateFrom(fetched);
        }

        private async Task DeletePlayersNotMonitoredAsync(CacheDbContext dbContext)
        {
            List<Context.CachedItems.CachedPlayer> cachedPlayers = await (
                from p in dbContext.Players
                join c in dbContext.Clans on p.ClanTag equals c.Tag
                into p_c
                from c2 in p_c.DefaultIfEmpty()
                where 
                    p.Download == false && 
                    (p.ExpiresAt ?? min) < DateTime.UtcNow.AddMinutes(-10) &&
                    (p.ClanTag == null || (c2 == null || c2.DownloadMembers == false))
                select p
            ).ToListAsync(_cancellationToken).ConfigureAwait(false);

            dbContext.RemoveRange(cachedPlayers);

            await dbContext.SaveChangesAsync(_cancellationToken);
        }
    }
}