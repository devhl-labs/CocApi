using System;
using System.Diagnostics;
using CocApiLibrary.Models;

namespace CocApiLibrary
{
    public class StoredItem
    {
        public DateTime DateTimeUTC { get; }
        public object DownloadedItem { get; }
        public DateTime Expires { get; }
        public TimeSpan TimeToDownload { get; }

        public string EncodedUrl { get; }

        public StoredItem(object downloadedItem, Stopwatch stopwatch, string encodedUrl)
        {
            TimeToDownload = stopwatch.Elapsed;

            DateTimeUTC = DateTime.UtcNow;

            DownloadedItem = downloadedItem;

            EncodedUrl = encodedUrl;

            Expires = DateTime.UtcNow.AddMinutes(5);

            if (downloadedItem is CurrentWarAPIModel currentWarAPIModel)
            {
                Expires = currentWarAPIModel.StartTimeUTC;

                if (currentWarAPIModel.State == Enums.State.InWar)
                {
                    Expires = DateTime.UtcNow.AddSeconds(15);
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
