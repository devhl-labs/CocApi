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
        internal static async Task<CachedPlayer> FromPlayerResponseAsync(string tag, PlayersClientBase playersCacheBase, PlayersApi playersApi)
        {
            try
            {
                ApiResponse<Player> apiResponse = await playersApi.GetPlayerResponseAsync(tag).ConfigureAwait(false);

                return new CachedPlayer(apiResponse, playersCacheBase.TimeToLive(apiResponse));
            }
            catch (Exception e) when (e is ApiException || e is TimeoutException)
            {
                return new CachedPlayer(tag, e, playersCacheBase.TimeToLive(e));
            }
        }

        public string Tag { get; }

        public bool Download { get; internal set; }

        private CachedPlayer(ApiResponse<Player> response, TimeSpan localExpiration) : base (response, localExpiration)
        {
            Tag = response.Data.Tag;
        }

        private CachedPlayer(string tag, Exception e, TimeSpan localExpiration) : base (e, localExpiration)
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
