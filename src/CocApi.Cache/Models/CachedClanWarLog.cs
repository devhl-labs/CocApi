using System;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWarLog : CachedItem<ClanWarLog>
    {
        public string Tag { get; internal set; } = string.Empty;


        public new void UpdateFrom(ApiResponse<ClanWarLog> responseItem, TimeSpan localExpiration)
        {
            base.UpdateFrom(responseItem, localExpiration);
        }

        public new void UpdateFrom(ApiException apiException, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiException, localExpiration);
        }

    }
}
