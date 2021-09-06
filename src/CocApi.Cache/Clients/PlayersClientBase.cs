using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public class PlayersClientBase : ClientBase, IHostedService
    {
        private readonly PlayersApi _playersApi;
        private readonly IOptions<MonitorOptions> _options;
        internal readonly PlayerMonitor PlayerMonitor;

        public PlayersClientBase(PlayersApi playersApi, CacheDbContextFactoryProvider provider, IOptions<MonitorOptions> options) : base (provider)
        {
            _playersApi = playersApi;
            _options = options;
            PlayerMonitor = new PlayerMonitor(provider, _playersApi, this, options);
        }

        public event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;

        internal ConcurrentDictionary<string, CachedPlayer?> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, Context.CachedItems.CachedPlayer?>();

        public async Task AddOrUpdateAsync(string tag, bool download = true)
            => await AddOrUpdateAsync(new string[] { tag }, download);

        public async Task AddOrUpdateAsync(IEnumerable<string> tags, bool download = true)
        {
            HashSet<string> formattedTags = new HashSet<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<Context.CachedItems.CachedPlayer> cachedPlayers = await dbContext.Players
                .Where(c => formattedTags.Contains(c.Tag))
                .ToListAsync()
                .ConfigureAwait(false);

            foreach (string formattedTag in formattedTags)
            {
                Context.CachedItems.CachedPlayer? trackedPlayer = cachedPlayers.FirstOrDefault(c => c.Tag == formattedTag);

                trackedPlayer ??= new Context.CachedItems.CachedPlayer(formattedTag); 

                trackedPlayer.Download = download;

                dbContext.Players.Update(trackedPlayer);
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(string tag)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            while (!UpdatingVillage.TryAdd(formattedTag, null))            
                await Task.Delay(250);            

            try
            {
                Context.CachedItems.CachedPlayer cachedPlayer = await dbContext.Players.FirstOrDefaultAsync(c => c.Tag == formattedTag);

                if (cachedPlayer != null)
                    dbContext.Players.Remove(cachedPlayer);

                await dbContext.SaveChangesAsync();
            }
            finally
            {
                UpdatingVillage.TryRemove(formattedTag, out _);
            }
        }

        public async Task<CachedPlayer> GetCachedPlayerAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Players
                .Where(i => i.Tag == formattedTag)
                .FirstAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<CachedPlayer?> GetCachedPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Players
                .Where(i => i.Tag == formattedTag)
                .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<Player> GetOrFetchPlayerAsync(string tag, CancellationToken? cancellationToken = default)
        {
            Player? result = (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

            if (result == null)
                result = await _playersApi.FetchPlayerAsync(tag, cancellationToken).ConfigureAwait(false);            

            return result;
        }

        public async Task<Player?> GetOrFetchPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            Player? result = (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

            if (result == null)
                result = await _playersApi.FetchPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);

            return result;
        }

        public async Task<List<CachedPlayer>> GetCachedPlayersAsync(IEnumerable<string> tags, CancellationToken? cancellationToken = default)
        {
            List<string> formattedTags = new List<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Players
                .AsNoTracking()
                .Where(i => formattedTags.Contains(i.Tag))
                .ToListAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public Task StartAsync(CancellationToken _)
        {
            if (_options.Value.Enabled)
                PlayerMonitor.StartAsync(_stopRequestedTokenSource.Token);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken _)
        {
            _stopRequestedTokenSource.Cancel();

            if (PlayerMonitor.RunTask != null)
                await PlayerMonitor.RunTask;
        }

        internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync(ApiResponse<Player> apiResponse)
        {
            try
            {
                TimeSpan result = await TimeToLiveAsync(apiResponse).ConfigureAwait(false);

                return result < TimeSpan.Zero
                    ? TimeSpan.Zero
                    : result;
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, e, "An error occurred while getting the time to live for an ApiResponse."));

                return TimeSpan.FromMinutes(0);
            }
        }

        internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync(Exception exception)
        {
            try
            {
                TimeSpan result = await TimeToLiveAsync(exception).ConfigureAwait(false);

                return result < TimeSpan.Zero
                    ? TimeSpan.Zero
                    : result;
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, e, "An error occurred while getting the time to live."));

                return TimeSpan.FromMinutes(0);
            }
        }

        protected virtual ValueTask<TimeSpan> TimeToLiveAsync(ApiResponse<Player> apiResponse)
            => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        protected virtual ValueTask<TimeSpan> TimeToLiveAsync(Exception exception)
            => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(0));

        public async Task<CachedPlayer> UpdateAsync(string tag, bool download = true)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            CachedPlayer cachedPlayer = await dbContext.Players
                .FirstOrDefaultAsync(v => v.Tag == formattedTag)
                .ConfigureAwait(false);

            if (cachedPlayer != null && cachedPlayer.Download == download)
                return cachedPlayer;

            if (cachedPlayer == null)
            {
                cachedPlayer = new CachedPlayer(formattedTag);
                dbContext.Players.Add(cachedPlayer);
            }

            cachedPlayer.Download = download;

            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return cachedPlayer;
        }

        internal async Task OnPlayerUpdatedAsync(PlayerUpdatedEventArgs eventArgs)
        {
            await Library.SendConcurrentEvent(this, () =>
            {
                PlayerUpdated?.Invoke(this, eventArgs).ConfigureAwait(false);
            },
            eventArgs.CancellationToken);
        }

        protected virtual bool HasUpdated(Player? stored, Player fetched) => !fetched.Equals(stored);

        internal bool HasUpdatedOrDefault(Player? stored, Player fetched)
        {
            try
            {
                return HasUpdated(stored, fetched);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, e, "An error occurred while checking if the player updated."));

                return !fetched.Equals(stored);
            }
        }
    }
}