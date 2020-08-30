using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedPlayer : CachedItem<Player>
    {
        internal static async Task<CachedPlayer> FromPlayerResponseAsync(string tag, PlayersCacheBase playersCacheBase, PlayersApi playersApi)
        {
            try
            {
                ApiResponse<Player> apiResponse = await playersApi.GetPlayerResponseAsync(tag).ConfigureAwait(false);

                return new CachedPlayer(apiResponse, playersCacheBase.TimeToLive(apiResponse));
            }
            catch (ApiException apiException)
            {
                return new CachedPlayer(tag, apiException, playersCacheBase.TimeToLive(apiException));
            }
        }

        public string Tag { get; }

        public bool Download { get; internal set; }

        private CachedPlayer(ApiResponse<Player> response, TimeSpan localExpiration) : base (response, localExpiration)
        {
            Tag = response.Data.Tag;
        }

        private CachedPlayer(string tag, ApiException apiException, TimeSpan localExpiration) : base (apiException, localExpiration)
        {
            Tag = tag;
        }

        internal CachedPlayer(string tag)
        {
            Tag = tag;
        }

        internal void UpdateFrom(CachedPlayer fetched)
        {
            if (ServerExpiration > fetched.ServerExpiration)
                return;

            base.UpdateFrom(fetched);
        }
    }
}
