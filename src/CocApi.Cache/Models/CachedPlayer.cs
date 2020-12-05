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

                return new CachedPlayer(apiResponse, await playersCacheBase.TimeToLiveAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e) when (e is ApiException || e is TimeoutException || e is TaskCanceledException)
            {
                return new CachedPlayer(tag, e, await playersCacheBase.TimeToLiveAsync(e).ConfigureAwait(false));
            }
        }

        public string Tag { get; }

        public bool Download { get; internal set; }

        public string? ClanTag { get; private set; }

        private CachedPlayer(ApiResponse<Player> response, TimeSpan localExpiration) : base (response, localExpiration)
        {
            Tag = response.Data.Tag;

            ClanTag = response.Data.Clan?.Tag;
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

            ClanTag = fetched.ClanTag;

            base.UpdateFrom(fetched);
        }
    }
}
