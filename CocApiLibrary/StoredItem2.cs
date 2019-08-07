using System;
using System.Diagnostics;
using CocApiLibrary.Models;

namespace CocApiLibrary
{
    public class StoredItem2<T>
    {
        public DateTime DateTimeUTC { get; set; }
        public T DownloadedItem { get; set; }
        public DateTime Expires { get; set; }
        //public TimeSpan TimeToDownload { get; set; }

        public string EncodedUrl { get; }

        public StoredItem2(T downloadedItem, string encodedUrl)
        {
            //TimeToDownload = stopwatch.Elapsed;

            DateTimeUTC = DateTime.UtcNow;

            DownloadedItem = downloadedItem;

            EncodedUrl = encodedUrl;

            Expires = DateTime.UtcNow.AddSeconds(15);

            if (downloadedItem is CurrentWarAPIModel currentWarAPIModel)
            {
                Expires = currentWarAPIModel.StartTimeUTC;

                if (currentWarAPIModel.State == Enums.State.InWar)
                {
                    Expires = DateTime.UtcNow.AddSeconds(15);
                }
                else if (currentWarAPIModel.State == Enums.State.WarEnded)
                {
                    Expires = DateTime.UtcNow.AddDays(5);
                }
            }
            else if(downloadedItem is LeagueWarAPIModel leagueWarAPIModel)
            {
                Expires = leagueWarAPIModel.StartTimeUTC;

                if (leagueWarAPIModel.State == Enums.State.InWar)
                {
                    Expires = DateTime.UtcNow.AddSeconds(15);
                }
                else if(leagueWarAPIModel.State == Enums.State.WarEnded)
                {
                    Expires = DateTime.UtcNow.AddDays(5);
                }
            }

            if (downloadedItem is LeagueGroupAPIModel)
            {
                Expires = DateTime.UtcNow.AddMinutes(5);
            }
        }

        public bool IsExpired()
        {
            if(DateTime.UtcNow > Expires)
            {
                return true;
            }
            return false;
        }
    }
}
