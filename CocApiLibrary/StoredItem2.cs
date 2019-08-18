//using System;
//using System.Diagnostics;
//using CocApiLibrary.Models;

//namespace CocApiLibrary
//{
//    public class StoredItem2<T> where T : class, new()
//    {
//        public DateTime DateTimeUTC { get; private set; } = DateTime.UtcNow;

//        private T _downloadedItem = new T();
        
//        public T DownloadedItem
//        {
//            get
//            {
//                return _downloadedItem;
//            }
        
//            set
//            {
//        		_downloadedItem = value;

//                DateTimeUTC = DateTime.UtcNow;

//                Expires = DateTime.UtcNow.AddSeconds(15);

//                if (_downloadedItem is CurrentWarAPIModel currentWarAPIModel)
//                {
//                    Expires = currentWarAPIModel.StartTimeUTC;

//                    if (currentWarAPIModel.State == Enums.State.InWar)
//                    {
//                        Expires = DateTime.UtcNow.AddSeconds(15);
//                    }
//                    else if (currentWarAPIModel.State == Enums.State.WarEnded)
//                    {
//                        Expires = DateTime.UtcNow.AddDays(5);
//                    }
//                }
//                else if (_downloadedItem is LeagueWarAPIModel leagueWarAPIModel)
//                {
//                    Expires = leagueWarAPIModel.StartTimeUTC;

//                    if (leagueWarAPIModel.State == Enums.State.InWar)
//                    {
//                        Expires = DateTime.UtcNow.AddSeconds(15);
//                    }
//                    else if (leagueWarAPIModel.State == Enums.State.WarEnded)
//                    {
//                        Expires = DateTime.UtcNow.AddDays(5);
//                    }
//                }

//                if (_downloadedItem is LeagueGroupAPIModel)
//                {
//                    Expires = DateTime.UtcNow.AddMinutes(5);
//                }
//            }
//        }

//        public DateTime Expires { get; private set; }

//        public string EncodedUrl { get; }

//        public StoredItem2(T downloadedItem, string encodedUrl)
//        {
//            DownloadedItem = downloadedItem;

//            EncodedUrl = encodedUrl;
//        }

//        public bool IsExpired()
//        {
//            if(DateTime.UtcNow > Expires)
//            {
//                return true;
//            }
//            return false;
//        }
//    }
//}
