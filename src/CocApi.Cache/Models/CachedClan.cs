using System;
using CocApi.Model;
using CocApi.Client;

namespace CocApi.Cache.Models
{
    public class CachedClan : CachedItem<Clan>
    {
        public string Tag { get; internal set; } = string.Empty;

        public bool Download { get; internal set; }

        public bool DownloadMembers { get; internal set; }

        public bool DownloadCurrentWar { get; internal set; }

        public bool DownloadCwl { get; internal set; }

        public CachedClan(ApiResponse<Clan> response, TimeSpan localExpiration) : base (response, localExpiration)
        {
            Tag = response.Data.Tag;
        }

        public CachedClan()
        {

        }

        internal new void UpdateFrom(ApiResponse<Clan> responseItem, TimeSpan localExpiration)
        {
            base.UpdateFrom(responseItem, localExpiration);
        }

        internal new void UpdateFrom(ApiException apiException, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiException, localExpiration);
        }
    }


}
