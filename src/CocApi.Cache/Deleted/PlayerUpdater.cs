//using CocApi.Cache.Models.Cache;
//using CocApi.Cache.Models.Villages;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.EntityFrameworkCore;
//using CocApi.Client;
//using Newtonsoft.Json;
//using CocApi.Model;

//namespace CocApi.Cache.Updaters
//{
//    public sealed class PlayerUpdater
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private readonly CocApiConfiguration _cocApiConfiguration;
//        private readonly CocApiClient _cocApi;

//        public event AsyncEventHandler<ChangedEventArgs<Player>>? PlayerUpdated;
//        public event AsyncEventHandler<ChangedEventArgs<Player>>? PlayerUpdatedNOT;


//        internal void OnPlayerUpdated(Player fetched, Player queued)
//            => PlayerUpdated?.Invoke(this, new ChangedEventArgs<Player>(fetched, queued));

//        internal void OnPlayerUpdatedNOT(Player fetched, Player queued)
//            => PlayerUpdatedNOT?.Invoke(this, new ChangedEventArgs<Player>(fetched, queued));

//        public PlayerUpdater(CocApiClient cocApi, IServiceProvider serviceProvider, CocApiConfiguration cocApiConfiguration)
//        {
//            _cocApi = cocApi;
//            _serviceProvider = serviceProvider;
//            _cocApiConfiguration = cocApiConfiguration;
//        }

//        private bool IsRunning { get; set; }

//        private bool StopRequested { get; set; }

//        private ConcurrentDictionary<string, CachedItem> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, CachedItem>();

//        public void Start()
//        {
//            Task.Run(async() =>
//            {
//                try
//                {
//                    if (IsRunning)
//                        return;

//                    IsRunning = true;

//                    StopRequested = false;

//                    _cocApi.OnLog(new LogEventArgs(nameof(PlayerUpdater), nameof(Start), LogLevel.Information));

//                    int villageId = 0;

//                    while (!StopRequested)
//                    {
//                        List<Task> tasks = new List<Task>();

//                        using var scope = _serviceProvider.CreateScope();

//                        CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

//                        List<CachedVillage> cachedVillages = await dbContext.Villages.Where(v =>
//                            v.Id > villageId).OrderBy(v => v.Id).Take(_cocApiConfiguration.ConcurrentUpdates).ToListAsync();

//                        for (int i = 0; i < cachedVillages.Count; i++)
//                            tasks.Add(UpdateVillage(cachedVillages[i].VillageTag, dbContext));

//                        await Task.WhenAll(tasks).ConfigureAwait(false);

//                        await dbContext.SaveChangesAsync();

//                        if (cachedVillages.Count < _cocApiConfiguration.ConcurrentUpdates)
//                            villageId = 0;
//                        else
//                            villageId = cachedVillages.Max(v => v.Id);

//                        await Task.Delay(_cocApiConfiguration.DelayBetweenUpdates).ConfigureAwait(false);
//                    }

//                    IsRunning = false;
//                }
//                catch (Exception e)
//                {
//                    _cocApi.OnLog(new ExceptionEventArgs(nameof(PlayerUpdater), nameof(Start), e));

//                    IsRunning = false;

//                    Start();
//                }
//            });
//        }

//        public async Task StopAsync()
//        {
//            StopRequested = true;

//            _cocApi.OnLog(new LogEventArgs(nameof(Updater), nameof(StopAsync), LogLevel.Information));

//            while (IsRunning)
//                await Task.Delay(50).ConfigureAwait(false);
//        }

//        private async Task UpdateVillage(string villageTag, CacheContext cacheContext)
//        {
//            CachedItem? cachedItem = await _cocApi.PlayersCache.GetWithHttpInfoAsync(villageTag).ConfigureAwait(false);

//            if (cachedItem != null && (cachedItem.IsServerExpired() == false || cachedItem.IsLocallyExpired() == false))
//                return;

//            //if (cachedItem != null && cachedItem.IsLocallyExpired() == false)
//            //    return;

//            cachedItem ??= new CachedItem();

//            Player? storedPlayer = null;

//            if (string.IsNullOrEmpty(cachedItem.Raw) == false)
//                storedPlayer = JsonConvert.DeserializeObject<Player>(cachedItem.Raw);

//            if (UpdatingVillage.TryAdd(villageTag, cachedItem) == false)
//                return;

//            try
//            {
//                ApiResponse<Player>? fetched = await _cocApi.PlayersApi.GetPlayerWithHttpInfoAsync(villageTag);

//                string downloadDate = fetched.Headers.First(h => h.Key == "Date").Value.First();
//                string cacheControl = fetched.Headers.First(h => h.Key == "Cache-Control").Value.First().Replace("max-age=", "");

//                cachedItem.DownloadDate = DateTime.ParseExact(downloadDate, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", new System.Globalization.CultureInfo("en-US"));

//                cachedItem.ServerExpirationDate = cachedItem.DownloadDate.AddSeconds(double.Parse(cacheControl));

//                cachedItem.LocalExpirationDate = cachedItem.DownloadDate.Add(_cocApiConfiguration.VillageTimeToLive);

//                cachedItem.Raw = fetched.RawContent;

//                cachedItem.Path = Village.Url(villageTag);

//                cacheContext.Items.Update(cachedItem);

//                Player fetchedPlayer = JsonConvert.DeserializeObject<Player>(cachedItem.Raw);

//                if (storedPlayer != null && _cocApi.IsEqual(storedPlayer, fetchedPlayer) == false)
//                    PlayerUpdated?.Invoke(this, new ChangedEventArgs<Player>(storedPlayer, fetchedPlayer));
//                else
//                    PlayerUpdatedNOT?.Invoke(this, new ChangedEventArgs<Player>(storedPlayer, fetchedPlayer));
//            }
//            finally
//            {
//                UpdatingVillage.TryRemove(villageTag, out _);
//            }
//        }
//    }
//}