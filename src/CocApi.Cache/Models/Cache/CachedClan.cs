using Dapper.SqlWriter;
//using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Wars;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocApi.Model;
using CocApi.Client;

namespace CocApi.Cache.Models.Cache
{
    public class CachedClan : CachedItem<Clan>
    {
        public string Tag { get; set; } = string.Empty;

        public bool Download { get; set; }

        public bool DownloadMembers { get; set; }

        public bool DownloadCurrentWar { get; set; }

        public bool DownloadCwl { get; set; }

        public CachedClan(ApiResponse<Clan> response, TimeSpan localExpiration) : base (response, localExpiration)
        {
            Tag = response.Data.Tag;
        }

        public CachedClan()
        {

        }

        internal new void UpdateFromResponse(ApiResponse<Clan> responseItem, TimeSpan localExpiration)
        {
            base.UpdateFromResponse(responseItem, localExpiration);
        }
    }


}
