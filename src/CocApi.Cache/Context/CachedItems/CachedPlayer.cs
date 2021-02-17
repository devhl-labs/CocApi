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

namespace CocApi.Cache.Context.CachedItems
{
    public class CachedPlayer : CachedItem<Player>
    {
        internal static async Task<CachedPlayer> FromPlayerResponseAsync(string tag, PlayersClientBase playersCacheBase, PlayersApi playersApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<Player> apiResponse = await playersApi.FetchPlayerResponseAsync(tag, cancellationToken).ConfigureAwait(false);

                return new CachedPlayer(tag, apiResponse, await playersCacheBase.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                return new CachedPlayer(tag, await playersCacheBase.TimeToLiveOrDefaultAsync(e).ConfigureAwait(false));
            }
        }

        private CachedPlayer(string tag, ApiResponse<Player> response, TimeSpan localExpiration) : base (response, localExpiration)
        {
            Tag = tag;

            ClanTag = response.Content?.Clan?.Tag;
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
            if (ExpiresAt > fetched.ExpiresAt)
                return;

            ClanTag = fetched.ClanTag;

            base.UpdateFrom(fetched);
        }











        private string _tag;

        public string Tag { get { return _tag; } internal set { _tag = CocApi.Clash.FormatTag(value); } }

        private string? _clanTag;

        public string? ClanTag { get { return _clanTag; } internal set { _clanTag = value == null ? null : CocApi.Clash.FormatTag(value); }
        }

        public int Id { get; internal set; }
    }
}
