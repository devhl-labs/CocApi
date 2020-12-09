using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CocApi.Cache.View
{
    public class WarWithLogStatus
    {
        //cached item
        public int Id { get; internal set; }

        public string RawContent { get; internal set; } = string.Empty;

        public DateTime Downloaded { get; internal set; }

        public DateTime ServerExpiration { get; internal set; }

        public DateTime LocalExpiration { get; internal set; }

        public HttpStatusCode StatusCode { get; internal set; }



        //war
        public string ClanTag { get; internal set; }

        public string OpponentTag { get; internal set; }

        public DateTime PreparationStartTime { get; internal set; }

        public DateTime EndTime { get; internal set; }

        public string? WarTag { get; internal set; }

        public WarState? State { get; internal set; }

        public bool IsFinal { get; internal set; }

        public DateTime? Season { get; private set; }

        public HttpStatusCode? StatusCodeOpponent { get; internal set; }

        public Announcements Announcements { get; internal set; }

        public WarType Type { get; internal set; }


        //clan
        public bool? IsWarLogPublic { get; internal set; }

        public bool? DownloadCurrentWar { get; internal set; }
    }
}
