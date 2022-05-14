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
    public sealed class PlayerService : ServiceBase
    {
        internal event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;


        internal Synchronizer Synchronizer { get; }
        internal IApiFactory ApiFactory { get; }
        public IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }
        internal TimeToLiveProvider TimeToLiveProvider { get; }


        public PlayerService(
            ILogger<PlayerService> logger,
            IServiceScopeFactory scopeFactory,
            TimeToLiveProvider timeToLiveProvider,
            Synchronizer synchronizer,
            IApiFactory apiFactory,
            IOptions<CacheOptions> options)
        : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.Players))
        {
            Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
            TimeToLiveProvider = timeToLiveProvider;
            Synchronizer = synchronizer;
            ApiFactory = apiFactory;
            Options = options;
        }


        protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            PlayerServiceOptions options = Options.Value.Players;

            using var scope = ScopeFactory.CreateScope();

            CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

            List<CachedPlayer> trackedPlayers = await dbContext.Players
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

            IPlayersApi playersApi = ApiFactory.Create<IPlayersApi>();

            try
            {
                foreach (CachedPlayer trackedPlayer in trackedPlayers)
                {
                    if (!Synchronizer.UpdatingVillage.TryAdd(trackedPlayer.Tag, trackedPlayer))
                        continue;

                    updatingTags.Add(trackedPlayer.Tag);

                    if (trackedPlayer.Download && trackedPlayer.IsExpired)
                        tasks.Add(MonitorPlayerAsync(playersApi, trackedPlayer, cancellationToken));
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

        private async Task MonitorPlayerAsync(IPlayersApi playersApi, CachedPlayer cachedPlayer, CancellationToken cancellationToken)
        {
            CachedPlayer fetched = await CachedPlayer
                .FromPlayerResponseAsync(cachedPlayer.Tag, TimeToLiveProvider, playersApi, cancellationToken)
                .ConfigureAwait(false);

            if (fetched.Content != null && CachedPlayer.HasUpdated(cachedPlayer, fetched) && PlayerUpdated != null)
                await PlayerUpdated(this, new PlayerUpdatedEventArgs(cachedPlayer.Content, fetched.Content, cancellationToken)).ConfigureAwait(false);

            cachedPlayer.UpdateFrom(fetched);
        }
    }
}