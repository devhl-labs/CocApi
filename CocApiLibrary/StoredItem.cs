using System;
using System.Diagnostics;
using CocApiStandardLibrary.Models;

namespace CocApiStandardLibrary
{
    public class StoredItem
    {
        public DateTime DateTimeUTC { get; }
        public object DownloadedItem { get; }
        public DateTime Expires { get; }
        public TimeSpan TimeToDownload { get; }

        public StoredItem(object downloadedItem, Stopwatch stopwatch)
        {
            TimeToDownload = stopwatch.Elapsed;

            DateTimeUTC = DateTime.UtcNow;

            DownloadedItem = downloadedItem;

            Expires = DateTime.UtcNow.AddMinutes(5);

            if (downloadedItem is CurrentWarAPIModel currentWarAPIModel)
            {
                Expires = currentWarAPIModel.StartTimeUTC;

                if (currentWarAPIModel.StateEnum == Enums.State.inWar)
                {
                    Expires = DateTime.UtcNow.AddSeconds(15);
                }
            }

            if (downloadedItem is LeagueGroupAPIModel)
            {
                Expires = DateTime.UtcNow.AddMinutes(5);
            }
        }
    }
}
