using System;
using System.Collections.Generic;
using System.Text;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWarLeagueGroup : CachedItem<ClanWarLeagueGroup>
    {
        public string Tag { get; internal set; }

        public DateTime? Season { get; internal set; }

        public ClanWarLeagueGroup.StateEnum? State { get; internal set; }

        internal new void UpdateFrom(ApiResponse<ClanWarLeagueGroup> apiResponse, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiResponse, localExpiration);

            Season = apiResponse?.Data.Season;

            State = apiResponse?.Data.State;
        }

        public new void UpdateFrom(ApiException apiException, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiException, localExpiration);
        }
    }
}
