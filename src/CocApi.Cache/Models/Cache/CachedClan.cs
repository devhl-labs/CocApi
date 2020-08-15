using Dapper.SqlWriter;
using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Wars;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocApi.Cache.Models.Cache
{
    public class CachedClan : DBObject
    {
        public string ClanTag { get; set; } = string.Empty;

        public bool DownloadClan { get; set; }

        public bool DownloadVillages { get; set; }

        public bool DownloadCurrentWar { get; set; }

        public bool DownloadCwl { get; set; }

        public int Id { get; set; }
    }
}
