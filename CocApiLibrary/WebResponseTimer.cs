using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary
{
    internal class WebResponseTimer
    {
        public readonly object DownloadedItrem;
        public readonly TimeSpan TimeSpan;
        public readonly DateTime DateTimeUTCCreated = DateTime.UtcNow;


        public WebResponseTimer(object downloadedObject, TimeSpan timeSpan)
        {
            DownloadedItrem = downloadedObject;
            TimeSpan = timeSpan;

        }

    }
}
