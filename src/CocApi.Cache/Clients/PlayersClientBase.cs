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

namespace CocApi.Cache
{
    public class PlayersClientBase : ClientBase
    {
        private readonly PlayersApi _playersApi;

        internal readonly PlayerMonitor PlayerMonitor;

        public PlayersClientBase(PlayersApi playersApi, IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs) 
            : base (dbContextFactory, dbContextArgs)
        {
            _playersApi = playersApi;

            PlayerMonitor = new PlayerMonitor(DbContextFactory, dbContextArgs, _playersApi, this);
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

        private Task? _playerMonitorTask;

        public Task StartAsync(CancellationToken _)
        {
            if (!Library.Monitors.Players.IsDisabled)
                _playerMonitorTask = PlayerMonitor.RunAsync(_stopRequestedTokenSource.Token);

            //Task.Run(() =>
            //{
            //    if (!Library.Monitors.Players.IsDisabled)
            //        Task.Run(() => PlayerMontitor.RunAsync(_stopRequestedTokenSource.Token));
            //});

            return Task.CompletedTask;
        }

        //internal bool HasUpdated(CachedPlayer stored, CachedPlayer fetched)
        //{
        //    if (stored.Content == null && fetched.Content != null)
        //        return true;

        //    if (stored.ExpiresAt > fetched.ExpiresAt)
        //        return false;

        //    if (stored.Content == null || fetched.Content == null)
        //        return false;

        //    return !fetched.Content.Equals(stored.Content);

        //    //return HasUpdated(stored.Data, fetched.Data);
        //}

        internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync(ApiResponse<Player> apiResponse)
        {
            try
            {
                return await TimeToLiveAsync(apiResponse).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return TimeSpan.FromMinutes(0);
            }
        }

        internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync(Exception exception)
        {
            try
            {
                return await TimeToLiveAsync(exception).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return TimeSpan.FromMinutes(0);
            }
        }

        public virtual ValueTask<TimeSpan> TimeToLiveAsync(ApiResponse<Player> apiResponse)
            => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        public virtual ValueTask<TimeSpan> TimeToLiveAsync(Exception exception)
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

        public async Task StopAsync(CancellationToken _)
        {
            _stopRequestedTokenSource.Cancel();

            if (_playerMonitorTask != null)
                await _playerMonitorTask;

            //Library.OnLog(this, new LogEventArgs(LogLevel.Information, "stopped"));
        }

        internal async Task OnPlayerUpdatedAsync(PlayerUpdatedEventArgs events)
        {
            await Library.ConcurrentEventsSemaphore.WaitAsync(events.CancellationToken);

            try
            {   
                PlayerUpdated?.Invoke(this, events).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on player updated.", e));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }
    }
}