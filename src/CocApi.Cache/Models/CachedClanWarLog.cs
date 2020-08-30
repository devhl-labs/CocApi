using System;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWarLog : CachedItem<ClanWarLog>
    {
        internal static async Task<CachedClanWarLog> FromClanWarLogResponseAsync(string tag, ClansCacheBase clansCacheBase, ClansApi clansApi)
        {
            try
            {
                ApiResponse<ClanWarLog> apiResponse = await clansApi.GetClanWarLogResponseAsync(tag);

                return new CachedClanWarLog(tag, apiResponse, clansCacheBase.ClanWarLogTimeToLive(apiResponse));
            }
            catch (ApiException apiException)
            {
                return new CachedClanWarLog(tag, apiException, clansCacheBase.ClanWarLogTimeToLive(apiException));
            }
        }

        public string Tag { get; internal set; } 

        internal CachedClanWarLog(string tag)
        {
            Tag = tag;
        }

        private CachedClanWarLog(string tag, ApiResponse<ClanWarLog> apiResponse, TimeSpan localExpiration)
        {
            Tag = tag;

            UpdateFrom(apiResponse, localExpiration);
        }

        private CachedClanWarLog(string tag, ApiException apiException, TimeSpan localExpiration)
        {
            Tag = tag;

            UpdateFrom(apiException, localExpiration);
        }

        internal void UpdateFrom(CachedClanWarLog fetched)
        {
            if (ServerExpiration > fetched.ServerExpiration)
                return;

            base.UpdateFrom(fetched);
        }
    }
}
