using CocApi.Api;
using CocApi.Cache.CocApi;
using CocApi.Cache.Models.Cache;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CocApi.Cache
{
    public class PlayersCache
    {
        private readonly CocApiClient _cocApi;
        private readonly CocApiConfiguration _cocApiConfiguration;
        private readonly IServiceProvider _services;
        private readonly PlayersApi _playersApi;

        public event AsyncEventHandler<ChangedEventArgs<Player>>? PlayerUpdated;

        internal void OnPlayerUpdated(Player stored, Player fetched)
            => PlayerUpdated?.Invoke(this, new ChangedEventArgs<Player>(stored, fetched));

        public async Task<CachedPlayer> AddAsync(string tag, bool download = true)
        {
            if (Clash.TryGetValidTag(tag, out string formattedTag) == false)
                throw new InvalidTagException(tag);

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

        public async Task<CachedPlayer> UpdateAsync(string tag, bool download = true)
        {
            if (Clash.TryGetValidTag(tag, out string formattedTag) == false)
                throw new InvalidTagException(tag);

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

        public PlayersCache(CocApiClient cocApi, CocApiConfiguration cocApiConfiguration, IServiceProvider serviceProvider, PlayersApi playersApi)
        {
            _cocApi = cocApi;
            _cocApiConfiguration = cocApiConfiguration;
            _services = serviceProvider;
            _playersApi = playersApi;
        }

        //public async Task<Player?> GetAsync(string tag) => await _cocApi.GetAsync<Player>(Player.Url(tag));

        //public async Task<CachedItem?> GetWithHttpInfoAsync(string tag) => await _cocApi.GetWithHttpInfoAsync(Player.Url(tag));

        internal async Task<Player> GetAsync(string tag)
        {
            CachedPlayer result = await GetWithHttpInfoAsync(tag);

            return result.Data;
        }

        public async Task<CachedPlayer> GetWithHttpInfoAsync(string tag)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Players.Where(i => i.Tag == tag).FirstAsync().ConfigureAwait(false);
        }

        public async Task<Player?> GetFirstOrDefaultAsync(string tag)
        {
            CachedPlayer? result = await GetWithHttpInfoFirstOrDefaultAsync(tag);

            return result?.Data;
        }

        public async Task<CachedPlayer?> GetWithHttpInfoFirstOrDefaultAsync(string tag)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Players.Where(i => i.Tag == tag).FirstOrDefaultAsync().ConfigureAwait(false);
        }












        private bool IsRunning { get; set; }

        private bool StopRequested { get; set; }

        internal ConcurrentDictionary<string, CachedPlayer?> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, CachedPlayer?>();

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
                            tasks.Add(UpdatePlayerAsync(cachedPlayers[i], dbContext));

                        await Task.WhenAll(tasks).ConfigureAwait(false);

                        await dbContext.SaveChangesAsync();

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

        internal async Task UpdatePlayerAsync(CachedPlayer cachedPlayer, CachedContext dbContext)
        {
            if (cachedPlayer.IsServerExpired() == false || cachedPlayer.IsLocallyExpired() == false)
                return;

            if (UpdatingVillage.TryAdd(cachedPlayer.Tag, cachedPlayer) == false)
                return;

            try
            {
                ApiResponse<Player> apiResponse = await _cocApi.PlayersApi.GetPlayerWithHttpInfoOrDefaultAsync(cachedPlayer.Tag);

                if (cachedPlayer.ServerExpiration >= apiResponse.ServerExpiration)
                    return;

                if (cachedPlayer.Data != null && _cocApi.IsEqual(cachedPlayer.Data, apiResponse.Data) == false)
                    PlayerUpdated?.Invoke(this, new ChangedEventArgs<Player>(cachedPlayer.Data, apiResponse.Data));

                cachedPlayer.UpdateFromResponse(apiResponse, _cocApiConfiguration.VillageTimeToLive);

                dbContext.Players.Update(cachedPlayer);
            }
            finally
            {
                UpdatingVillage.TryRemove(cachedPlayer.Tag, out _);
            }


            //CachedItem? cachedItem = await _cocApi.PlayersCache.GetWithHttpInfoAsync(tag).ConfigureAwait(false);

            //if (cachedItem != null && (cachedItem.IsServerExpired() == false || cachedItem.IsLocallyExpired() == false))
            //    return;

            //cachedItem ??= new CachedItem();

            //if (UpdatingVillage.TryAdd(tag, cachedItem) == false)
            //    return;

            //Player? storedPlayer = null;

            //if (string.IsNullOrEmpty(cachedItem.Raw) == false)
            //    storedPlayer = JsonConvert.DeserializeObject<Player>(cachedItem.Raw);

            //try
            //{
            //    ApiResponse<Player> apiResponse = await _cocApi.PlayersApi.GetPlayerWithHttpInfoAsync(tag);

            //    CachedItem responseItem = apiResponse.ToCachedItem(_cocApiConfiguration.VillageTimeToLive, Player.Url(tag));

            //    if (cachedItem.ServerExpirationDate == responseItem.ServerExpirationDate)
            //        return;

            //    cachedItem.UpdateFromResponse(responseItem);

            //    dbContext.Items.Update(cachedItem);

            //    Player fetchedPlayer = JsonConvert.DeserializeObject<Player>(apiResponse.RawContent);

            //    if (storedPlayer != null && _cocApi.IsEqual(storedPlayer, fetchedPlayer) == false)
            //        PlayerUpdated?.Invoke(this, new ChangedEventArgs<Player>(storedPlayer, fetchedPlayer));
            //}
            //finally
            //{
            //    UpdatingVillage.TryRemove(tag, out _);
            //}
        }
    }
}