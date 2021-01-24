using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedPlayer : CachedItem<Player>
    {
        internal static async Task<CachedPlayer> FromPlayerResponseAsync(string tag, PlayersClientBase playersCacheBase, PlayersApi playersApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<Player> apiResponse = await playersApi.GetPlayerResponseAsync(tag, cancellationToken).ConfigureAwait(false);

                return new CachedPlayer(tag, apiResponse, await playersCacheBase.TimeToLiveAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                return new CachedPlayer(tag, await playersCacheBase.TimeToLiveAsync(e).ConfigureAwait(false));
            }
        }

        public string Tag { get; }

        public bool Download { get; internal set; }

        public string? ClanTag { get; private set; }

        private CachedPlayer(string tag, ApiResponse<Player> response, TimeSpan localExpiration) : base (response, localExpiration)
        {
            Tag = tag;

            ClanTag = response.Data?.Clan?.Tag;
        }

        private CachedPlayer(string tag, TimeSpan localExpiration) : base (localExpiration)
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

            ClanTag = fetched.ClanTag;

            base.UpdateFrom(fetched);
        }
    }
}
