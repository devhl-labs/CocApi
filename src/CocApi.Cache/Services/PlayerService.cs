﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using CocApi.Cache.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public sealed class PlayerService : PerpetualService<PlayerService>
    {
        internal event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;


        internal Synchronizer Synchronizer { get; }
        internal PlayersApi PlayersApi { get; }
        internal IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }
        internal TimeToLiveProvider TimeToLiveProvider { get; }


        public PlayerService(
            CacheDbContextFactoryProvider provider, 
            TimeToLiveProvider timeToLiveProvider,
            Synchronizer synchronizer,
            PlayersApi playersApi,
            IOptions<CacheOptions> options) 
        : base(provider, options.Value.Players.DelayBeforeExecution, options.Value.Players.DelayBetweenExecutions)
        {
            Instantiated = Library.EnsureSingleton(Instantiated);
            TimeToLiveProvider = timeToLiveProvider;
            Synchronizer = synchronizer;
            PlayersApi = playersApi;
            Options = options;
        }


        private protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            ServiceOptions options = Options.Value.Players;

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
                    if (!Synchronizer.UpdatingVillage.TryAdd(trackedPlayer.Tag, trackedPlayer))
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
                    Synchronizer.UpdatingVillage.TryRemove(tag, out _);
            }
        }

        private async Task MonitorPlayerAsync(CachedPlayer cachedPlayer, CancellationToken cancellationToken)
        {
            CachedPlayer fetched = await CachedPlayer
                .FromPlayerResponseAsync(cachedPlayer.Tag, TimeToLiveProvider, PlayersApi, cancellationToken)
                .ConfigureAwait(false);

            if (fetched.Content != null && CachedPlayer.HasUpdated(cachedPlayer, fetched) && PlayerUpdated != null)
                await PlayerUpdated(this, new PlayerUpdatedEventArgs(cachedPlayer.Content, fetched.Content, cancellationToken)).ConfigureAwait(false);

            cachedPlayer.UpdateFrom(fetched);
        }
    }
}