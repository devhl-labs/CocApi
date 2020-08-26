using System;
using System.Collections.Generic;
using System.Text;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWarLeagueGroup : CachedItem<ClanWarLeagueGroup>
    {
        public string Tag { get; set; }

        public DateTime? Season { get; set; }

        public ClanWarLeagueGroup.StateEnum? State { get; set; }

        public void UpdateFrom(ApiResponse<ClanWarLeagueGroup>? apiResponse, TimeSpan localExpiration)
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
