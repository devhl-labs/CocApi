using CocApi.Api;
using CocApi.Cache.Models.Cache;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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

        public async Task AddAsync(string tag)
        {
            if (Clash.TryGetValidTag(tag, out string formattedTag) == false)
                throw new InvalidTagException(tag);

            using var scope = _services.CreateScope();

            CacheContext cacheContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedVillage cachedVillage = await cacheContext.Villages.Where(v => v.VillageTag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            cachedVillage ??= new CachedVillage();

            cachedVillage.VillageTag = formattedTag;

            cacheContext.Villages.Update(cachedVillage);

            await cacheContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveAsync(string tag)
        {
            if (Clash.TryGetValidTag(tag, out string formattedTag) == false)
                throw new InvalidTagException(tag);

            using var scope = _services.CreateScope();

            CacheContext cacheContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedVillage cachedVillage = await cacheContext.Villages.FirstOrDefaultAsync(villageTag =>
                villageTag.VillageTag == formattedTag).ConfigureAwait(false);

            if (cachedVillage == null)
                return;

            cacheContext.Remove(cachedVillage);

            await cacheContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public PlayersCache(CocApiClient cocApi, CocApiConfiguration cocApiConfiguration, IServiceProvider serviceProvider, PlayersApi playersApi)
        {
            _cocApi = cocApi;
            _cocApiConfiguration = cocApiConfiguration;
            _services = serviceProvider;
            _playersApi = playersApi;
        }

        public async Task<Player?> GetAsync(string tag) => await _cocApi.GetAsync<Player>(Player.Url(tag));

        public async Task<CachedItem?> GetWithHttpInfoAsync(string tag) => await _cocApi.GetWithHttpInfoAsync(Player.Url(tag));













        private bool IsRunning { get; set; }

        private bool StopRequested { get; set; }

        internal ConcurrentDictionary<string, CachedItem?> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, CachedItem?>();

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

                        CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                        List<CachedVillage> cachedVillages = await dbContext.Villages.Where(v =>
                            v.Id > id).OrderBy(v => v.Id).Take(_cocApiConfiguration.ConcurrentUpdates).ToListAsync().ConfigureAwait(false);

                        for (int i = 0; i < cachedVillages.Count; i++)
                            tasks.Add(UpdatePlayerAsync(cachedVillages[i].VillageTag, dbContext));

                        await Task.WhenAll(tasks).ConfigureAwait(false);

                        await dbContext.SaveChangesAsync();

                        if (cachedVillages.Count < _cocApiConfiguration.ConcurrentUpdates)
                            id = 0;
                        else
                            id = cachedVillages.Max(v => v.Id);

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

        internal async Task UpdatePlayerAsync(string tag, CacheContext dbContext)
        {
            CachedItem? cachedItem = await _cocApi.PlayersCache.GetWithHttpInfoAsync(tag).ConfigureAwait(false);

            if (cachedItem != null && (cachedItem.IsServerExpired() == false || cachedItem.IsLocallyExpired() == false))
                return;

            cachedItem ??= new CachedItem();

            if (UpdatingVillage.TryAdd(tag, cachedItem) == false)
                return;

            Player? storedPlayer = null;

            if (string.IsNullOrEmpty(cachedItem.Raw) == false)
                storedPlayer = JsonConvert.DeserializeObject<Player>(cachedItem.Raw);

            try
            {
                ApiResponse<Player> apiResponse = await _cocApi.PlayersApi.GetPlayerWithHttpInfoAsync(tag);

                CachedItem responseItem = apiResponse.ToCachedItem(_cocApiConfiguration.VillageTimeToLive, Player.Url(tag));

                if (cachedItem.ServerExpirationDate == responseItem.ServerExpirationDate)
                    return;

                cachedItem.DownloadDate = responseItem.DownloadDate;
                cachedItem.ServerExpirationDate = responseItem.ServerExpirationDate;
                cachedItem.LocalExpirationDate = responseItem.LocalExpirationDate;
                cachedItem.Raw = responseItem.Raw;
                cachedItem.Path = responseItem.Path;

                dbContext.Items.Update(cachedItem);

                Player fetchedPlayer = JsonConvert.DeserializeObject<Player>(apiResponse.RawContent);

                if (storedPlayer != null && _cocApi.IsEqual(storedPlayer, fetchedPlayer) == false)
                    PlayerUpdated?.Invoke(this, new ChangedEventArgs<Player>(storedPlayer, fetchedPlayer));
            }
            finally
            {
                UpdatingVillage.TryRemove(tag, out _);
            }
        }
    }
}