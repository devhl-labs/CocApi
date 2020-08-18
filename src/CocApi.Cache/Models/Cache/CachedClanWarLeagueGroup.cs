using System;
using System.Collections.Generic;
using System.Text;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models.Cache
{
    public class CachedClanWarLeagueGroup : CachedItem<ClanWarLeagueGroup>
    {
        public string Tag { get; set; }

        public string? Season { get; set; }

        public ClanWarLeagueGroup.StateEnum? State { get; set; }

        public void UpdateFrom(ApiResponse<ClanWarLeagueGroup> apiResponse, TimeSpan localExpiration)
        {
            base.UpdateFromResponse(apiResponse, localExpiration);

            Season = apiResponse.Data.Season;

            State = apiResponse.Data.State;
        }
    }
}
