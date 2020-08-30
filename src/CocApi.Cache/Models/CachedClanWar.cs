using System;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWar : CachedItem<ClanWar>
    {
        internal static async Task<CachedClanWar> FromCurrentWarResponseAsync(string tag, ClansCacheBase clansCacheBase, ClansApi clansApi)
        {
            try
            {
                ApiResponse<ClanWar> apiResponse = await clansApi.GetCurrentWarResponseAsync(tag);

                return new CachedClanWar(tag, apiResponse, clansCacheBase.ClanWarTimeToLive(apiResponse));
            }
            catch (ApiException apiException)
            {
                return new CachedClanWar(tag, apiException, clansCacheBase.ClanWarTimeToLive(apiException));
            }
        }

        public string Tag { get; internal set; }

        public ClanWar.StateEnum? State { get; internal set; }

        public DateTime PreparationStartTime { get; internal set; }

        public ClanWar.TypeEnum Type { get; internal set; }

        internal CachedClanWar(string tag)
        {
            Tag = tag;
        }

        private CachedClanWar(string clanTag, ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiResponse, localExpiration);

            State = apiResponse?.Data.State;

            PreparationStartTime = apiResponse?.Data.PreparationStartTime ?? PreparationStartTime;

            Tag = clanTag;
        }

        private CachedClanWar(string clanTag, ApiException apiException, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiException, localExpiration);

            Tag = clanTag;
        }

        internal void UpdateFrom(CachedClanWar fetched)
        {
            if (ServerExpiration > fetched.ServerExpiration)
                return;

            base.UpdateFrom(fetched);

            State = fetched.State;

            PreparationStartTime = fetched.PreparationStartTime;

            //Type = fetched.Type;  //only do this at the beginning of war
        }
    }
}
