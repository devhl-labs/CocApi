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
            catch (Exception e) when (e is ApiException || e is TimeoutException)
            {
                return new CachedClanWarLeagueGroup(tag, e, clansCacheBase.ClanWarLeagueGroupTimeToLive(e));
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

        private CachedClanWarLeagueGroup(string tag, Exception e, TimeSpan localExpiration)
            : this(tag)
        {
            UpdateFrom(e, localExpiration);
        }

        public GroupState? State { get; internal set; }

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
