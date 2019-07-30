//this was an attempt to have the api polling loop occur in the library and fire events when changes are noticed
//this could be a future update



//using CocApiLibrary.Exceptions;
//using CocApiLibrary.Models;
//using System.Collections.Concurrent;
//using System.Threading.Tasks;

//namespace CocApiLibrary
//{
//    internal class ClanStore
//    {
//        internal readonly ConcurrentDictionary<string, StoredItem<ClanAPIModel>> clans = new ConcurrentDictionary<string, StoredItem<ClanAPIModel>>();
//        private readonly WebResponse _webResponse;
//        private readonly CocApi _cocApi;
//        private bool _update = false;

//        public ClanStore(WebResponse webResponse, CocApi cocApi)
//        {
//            _webResponse = webResponse;
//            _cocApi = cocApi;
//        }

//        public async Task<ClanAPIModel> GetOrDownloadAsync(string clanTag, string encodedUrl)
//        {
//            clans.TryGetValue(clanTag, out StoredItem<ClanAPIModel> storedClan);

//            if (storedClan != null && !storedClan.IsExpired())
//            {
//                return storedClan.DownloadedItem;
//            }

//            StoredItem<ClanAPIModel> downloadedClan = await _webResponse.GetWebResponse<ClanAPIModel>(_cocApi, encodedUrl);

//            if (downloadedClan == null && storedClan != null)
//            {
//                return storedClan.DownloadedItem;
//            }

//            if (downloadedClan != null)
//            {
//                if (storedClan != null)
//                {
//                    clans.TryRemove(storedClan.DownloadedItem.Tag, out _);
//                }

//                clans.TryAdd(downloadedClan.DownloadedItem.Tag, downloadedClan);

//                return downloadedClan.DownloadedItem;
//            }
//            else if (storedClan != null) //return the expired item
//            {
//                return storedClan.DownloadedItem;
//            }
//            else
//            {
//                throw new CocApiException("No matching results found.");
//            }
//        }

//        public void Update(bool update)
//        {
//            _update = update;

//            Task.Run(async () =>
//            {
//                await Update();
//            });
//        }

//        private async Task Update()
//        {
//            while (_update)
//            {
//                foreach(var clanKVP in clans)
//                {
//                    System.Console.WriteLine($"downloading war {clanKVP.Key}");

//                    await clanKVP.Value.DownloadedItem.DownloadCurrentWarAsync();

//                    if (!_update)
//                    {
//                        break;
//                    }
//                }
//            }
//        }
//    }
//}
