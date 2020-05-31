using Dapper.SqlWriter;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.War;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace devhl.CocApi.Models.Cache
{
    public class CachedClan : DBObject
    {
        public string ClanTag { get; set; } = string.Empty;

        public string? ClanJson { get; set; }

        public DateTime ClanUpdatesAt { get; set; }

        public bool DownloadClan { get; set; }

        public bool DownloadVillages { get; set; }

        public bool DownloadCurrentWar { get; set; }

        public bool DownloadCwl { get; set; }

        public bool IsWarLogPublic { get; set; }

        public DateTime WarUpdatesAt { get; set; }

        public Clan.Clan? ToClan()
        {
            if (ClanJson == null)
                return null;

            return JsonConvert.DeserializeObject<Clan.Clan>(ClanJson);
        }
    }
}
