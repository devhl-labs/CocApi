using System;
using System.Collections.Generic;
using System.Text;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models.Cache
{
    public class CachedClanWarLog : CachedItem<ClanWarLog>
    {
        public string Tag { get; set; } = string.Empty;


        public new void UpdateFromResponse(ApiResponse<ClanWarLog>? responseItem, TimeSpan localExpiration)
        {
            base.UpdateFromResponse(responseItem, localExpiration);
        }

    }
}
