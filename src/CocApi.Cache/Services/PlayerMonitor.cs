using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using CocApi.Cache.Services;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public sealed class PlayerMonitor : PerpetualMonitor<PlayerMonitor>
    {
        public event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;

        private readonly Synchronizer _synchronizer;
        private readonly PlayersApi _playersApi;
        private readonly IOptions<MonitorOptions> _options;
        private DateTime _deletedUnmonitoredPlayers = DateTime.UtcNow;

        public TimeToLiveProvider TimeToLiveProvider { get; }

        public PlayerMonitor(
            CacheDbContextFactoryProvider provider, 
            TimeToLiveProvider timeToLiveProvider,
            Synchronizer synchronizer,
            PlayersApi playersApi,
            IOptions<MonitorOptions> options) : base(provider)
        {
            TimeToLiveProvider = timeToLiveProvider;
            _synchronizer = synchronizer;
            _playersApi = playersApi;
            _options = options;
        }

        //protected override async Task PollAsync()
        //{
        //    MonitorOptions options = _options.Value;

        //    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

        //    List<Context.CachedItems.CachedPlayer> trackedPlayers = await dbContext.Players
        //        .Where(p =>
        //            (p.ExpiresAt ?? min) < expires &&
        //            (p.KeepUntil ?? min) < now &&
        //            p.Download &&
        //            p.Id > _id)
        //        .OrderBy(p => p.Id)
        //        .Take(options.ConcurrentUpdates)
        //        .ToListAsync(cancellationToken)
        //        .ConfigureAwait(false);

        //    _id = trackedPlayers.Count == options.ConcurrentUpdates
        //        ? trackedPlayers.Max(c => c.Id)
        //        : int.MinValue;

        //    List<Task> tasks = new();

        //    HashSet<string> updatingTags = new();

        //    try
        //    {
        //        foreach (Context.CachedItems.CachedPlayer trackedPlayer in trackedPlayers)
        //        {
        //            if (!_playersClientBase.UpdatingVillage.TryAdd(trackedPlayer.Tag, trackedPlayer))
        //                continue;

        //            updatingTags.Add(trackedPlayer.Tag);

        //            if (trackedPlayer.Download && trackedPlayer.IsExpired)
        //                tasks.Add(MonitorPlayerAsync(trackedPlayer));
        //        }

        //        await Task.WhenAll(tasks);

        //        await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        //    }
        //    finally
        //    {
        //        foreach (string tag in updatingTags)
        //            _playersClientBase.UpdatingVillage.TryRemove(tag, out _);
        //    }

        //    if (_deletedUnmonitoredPlayers < DateTime.UtcNow.AddMinutes(-10))
        //    {
        //        _deletedUnmonitoredPlayers = DateTime.UtcNow;

        //        await DeletePlayersNotMonitoredAsync(dbContext).ConfigureAwait(false);
        //    }

        //    if (_id == int.MinValue)
        //        await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
        //    else
        //        await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
        //}

        private async Task MonitorPlayerAsync(CachedPlayer cachedPlayer, CancellationToken cancellationToken)
        {
            CachedPlayer fetched = await CachedPlayer
                .FromPlayerResponseAsync(cachedPlayer.Tag, TimeToLiveProvider, _playersApi, cancellationToken)
                .ConfigureAwait(false);

            if (fetched.Content != null && PlayerUpdated != null)
                await PlayerUpdated(this, new PlayerUpdatedEventArgs(cachedPlayer.Content, fetched.Content, cancellationToken)).ConfigureAwait(false);

            cachedPlayer.UpdateFrom(fetched);
        }

        private async Task DeletePlayersNotMonitoredAsync(CacheDbContext dbContext, CancellationToken cancellationToken)
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
            ).ToListAsync(cancellationToken).ConfigureAwait(false);

            dbContext.RemoveRange(cachedPlayers);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            MonitorOptions options = _options.Value;

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<Context.CachedItems.CachedPlayer> trackedPlayers = await dbContext.Players
                .Where(p =>
                    (p.ExpiresAt ?? min) < expires &&
                    (p.KeepUntil ?? min) < now &&
                    p.Download &&
                    p.Id > _id)
                .OrderBy(p => p.Id)
                .Take(options.ConcurrentUpdates)
                .ToListAsync(cancellationToken)
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
                    if (!_synchronizer.UpdatingVillage.TryAdd(trackedPlayer.Tag, trackedPlayer))
                        continue;

                    updatingTags.Add(trackedPlayer.Tag);

                    if (trackedPlayer.Download && trackedPlayer.IsExpired)
                        tasks.Add(MonitorPlayerAsync(trackedPlayer, cancellationToken));
                }

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingTags)
                    _synchronizer.UpdatingVillage.TryRemove(tag, out _);
            }

            if (_deletedUnmonitoredPlayers < DateTime.UtcNow.AddMinutes(-10))
            {
                _deletedUnmonitoredPlayers = DateTime.UtcNow;

                await DeletePlayersNotMonitoredAsync(dbContext, cancellationToken).ConfigureAwait(false);
            }

            // todo what am i doing with these
            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
        }
    }
}