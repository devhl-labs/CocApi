using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWarLeagueGroup : CachedItem<ClanWarLeagueGroup>
    {
        internal static async Task<CachedClanWarLeagueGroup> FromClanWarLeagueGroupResponseAsync(string tag, ClansCacheBase clansCacheBase, ClansApi clansApi)
        {
            try
            {
                ApiResponse<ClanWarLeagueGroup> apiResponse = await clansApi.GetClanWarLeagueGroupResponseAsync(tag).ConfigureAwait(false);

                return new CachedClanWarLeagueGroup(apiResponse, clansCacheBase.ClanWarLeagueGroupTimeToLive(apiResponse));
            }
            catch (ApiException apiException)
            {
                return new CachedClanWarLeagueGroup(tag, apiException, clansCacheBase.ClanWarLeagueGroupTimeToLive(apiException));
            }
        }

        public string Tag { get; internal set; }

        public DateTime Season { get; internal set; }

        internal CachedClanWarLeagueGroup(string tag)
        {
            Tag = tag;
        }

        private CachedClanWarLeagueGroup(ApiResponse<ClanWarLeagueGroup> apiResponse, TimeSpan localExpiration)
            : this(apiResponse.Data.Tag)
        {
            UpdateFrom(apiResponse, localExpiration);

            Season = apiResponse.Data.Season;

            State = apiResponse?.Data.State;
        }

        private CachedClanWarLeagueGroup(string tag, ApiException apiException, TimeSpan localExpiration)
            : this(tag)
        {
            UpdateFrom(apiException, localExpiration);
        }

        public ClanWarLeagueGroup.StateEnum? State { get; internal set; }

        internal void UpdateFrom(CachedClanWarLeagueGroup fetched)
        {
            if (ServerExpiration > fetched.ServerExpiration)
                return;

            base.UpdateFrom(fetched);

            if (fetched.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Season = fetched.Season;

                State = fetched.State;
            }
        }
    }
}
