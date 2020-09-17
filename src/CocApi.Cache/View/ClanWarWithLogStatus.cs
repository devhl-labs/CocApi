using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CocApi.Cache.View
{
    public class ClanWarWithLogStatus
    {
        //cached item
        public int Id { get; internal set; }

        public string RawContent { get; internal set; } = string.Empty;

        public DateTime Downloaded { get; internal set; }

        public DateTime ServerExpiration { get; internal set; }

        public DateTime LocalExpiration { get; internal set; }

        public HttpStatusCode StatusCode { get; internal set; }


        //clan war
        public string Tag { get; internal set; }

        public WarState? State { get; internal set; }

        public DateTime PreparationStartTime { get; internal set; }

        public WarType Type { get; internal set; }


        //clan
        public bool IsWarLogPublic { get; internal set; }

        public bool DownloadCurrentWar { get; internal set; }
    }
}
