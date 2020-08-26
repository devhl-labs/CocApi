using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public class PlayersCache
    {
        private readonly CocApiClient _cocApi;
        private readonly CocApiConfiguration _cocApiConfiguration;
        private readonly PlayersApi _playersApi;
        private readonly IServiceProvider _services;

        public PlayersCache(CocApiClient cocApi, CocApiConfiguration cocApiConfiguration, IServiceProvider serviceProvider, PlayersApi playersApi)
        {
            _cocApi = cocApi;
            _cocApiConfiguration = cocApiConfiguration;
            _services = serviceProvider;
            _playersApi = playersApi;
        }

        public event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;

        internal ConcurrentDictionary<string, CachedPlayer?> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, CachedPlayer?>();

        private bool IsRunning { get; set; }

        private bool StopRequested { get; set; }

        public async Task<CachedPlayer> AddAsync(string tag, bool download = true)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext cacheContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedPlayer cachedPlayer = await cacheContext.Players.Where(v => v.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            if (cachedPlayer != null)
                return cachedPlayer;

            cachedPlayer = new CachedPlayer
            {
                Tag = formattedTag,
                Download = download
            };
            cacheContext.Players.Update(cachedPlayer);

            await cacheContext.SaveChangesAsync().ConfigureAwait(false);

            return cachedPlayer;
        }

        public async Task<CachedPlayer> GetCacheAsync(string tag)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Players.Where(i => i.Tag == tag).FirstAsync().ConfigureAwait(false);
        }

        public async Task<CachedPlayer?> GetCacheOrDefaultAsync(string tag)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Players.Where(i => i.Tag == tag).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<Player> GetOrFetchPlayerAsync(string tag)
        {
            return (await GetCacheOrDefaultAsync(tag).ConfigureAwait(false))?.Data
                ?? await _playersApi.GetPlayerAsync(tag).ConfigureAwait(false);
        }

        public async Task<Player?> GetOrFetchPlayerOrDefaultAsync(string tag)
        {
            return (await GetCacheOrDefaultAsync(tag).ConfigureAwait(false))?.Data
                ?? await _playersApi.GetPlayerOrDefaultAsync(tag).ConfigureAwait(false);
        }

        public async Task<Player?> GetPlayerOrDefaultAsync(string tag)
        {
            CachedPlayer? result = await GetCacheOrDefaultAsync(tag);

            return result?.Data;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (IsRunning)
                        return;

                    IsRunning = true;

                    StopRequested = false;

                    _cocApi.OnLog(new LogEventArgs(nameof(PlayersCache), nameof(Start), LogLevel.Information));

                    int id = 0;

                    while (!StopRequested)
                    {
                        List<Task> tasks = new List<Task>();

                        using var scope = _services.CreateScope();

                        CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                        List<CachedPlayer> cachedPlayers = await dbContext.Players.Where(v =>
                            v.Id > id).OrderBy(v => v.Id).Take(_cocApiConfiguration.ConcurrentUpdates).ToListAsync().ConfigureAwait(false);

                        for (int i = 0; i < cachedPlayers.Count; i++)
                            tasks.Add(UpdatePlayerAsync(cachedPlayers[i]));

                        await Task.WhenAll(tasks).ConfigureAwait(false);

                        if (cachedPlayers.Count < _cocApiConfiguration.ConcurrentUpdates)
                            id = 0;
                        else
                            id = cachedPlayers.Max(v => v.Id);

                        await Task.Delay(_cocApiConfiguration.DelayBetweenUpdates).ConfigureAwait(false);
                    }

                    IsRunning = false;
                }
                catch (Exception e)
                {
                    _cocApi.OnLog(new ExceptionEventArgs(nameof(PlayersCache), nameof(Start), e));

                    IsRunning = false;

                    Start();
                }
            });
        }

        public async Task StopAsync()
        {
            StopRequested = true;

            _cocApi.OnLog(new LogEventArgs(nameof(PlayersCache), nameof(StopAsync), LogLevel.Information));

            while (IsRunning)
                await Task.Delay(500).ConfigureAwait(false);
        }

        public async Task<CachedPlayer> UpdateAsync(string tag, bool download = true)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext cacheContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedPlayer cachedPlayer = await cacheContext.Players.Where(v => v.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            if (cachedPlayer != null && cachedPlayer.Download == download)
                return cachedPlayer;

            cachedPlayer ??= new CachedPlayer();
            cachedPlayer.Tag = formattedTag;
            cachedPlayer.Download = download;
            cacheContext.Players.Update(cachedPlayer);

            await cacheContext.SaveChangesAsync().ConfigureAwait(false);

            return cachedPlayer;
        }

        internal async Task<Player> GetAsync(string tag)
        {
            CachedPlayer result = await GetCacheAsync(tag);

            return result.Data;
        }

        internal void OnPlayerUpdated(Player stored, Player fetched)
                                                                                                                    => PlayerUpdated?.Invoke(this, new PlayerUpdatedEventArgs(stored, fetched));

        internal async Task UpdatePlayerAsync(CachedPlayer cachedPlayer)
        {
            if (cachedPlayer.IsServerExpired() == false || cachedPlayer.IsLocallyExpired() == false)
                return;

            if (UpdatingVillage.TryAdd(cachedPlayer.Tag, cachedPlayer) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                ApiResponse<Player> apiResponse = await _cocApi.PlayersApi.GetPlayerWithHttpInfoOrDefaultAsync(cachedPlayer.Tag);

                if (cachedPlayer.ServerExpiration >= apiResponse.ServerExpiration)
                    return;

                if (cachedPlayer.Data != null && _cocApi.IsEqual(cachedPlayer.Data, apiResponse.Data) == false)
                    OnPlayerUpdated(cachedPlayer.Data, apiResponse.Data);

                cachedPlayer.UpdateFromResponse(apiResponse, _cocApiConfiguration.VillageTimeToLive);

                dbContext.Players.Update(cachedPlayer);

                await dbContext.SaveChangesAsync();
            }
            finally
            {
                UpdatingVillage.TryRemove(cachedPlayer.Tag, out _);
            }
        }
    }
}