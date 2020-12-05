using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWarLeagueGroup : CachedItem<ClanWarLeagueGroup>
    {
        internal static async Task<CachedClanWarLeagueGroup> FromClanWarLeagueGroupResponseAsync(string tag, ClansClientBase clansCacheBase, ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<ClanWarLeagueGroup> apiResponse = await clansApi.GetClanWarLeagueGroupResponseAsync(tag, cancellationToken).ConfigureAwait(false);

                return new CachedClanWarLeagueGroup(apiResponse, await clansCacheBase.ClanWarLeagueGroupTimeToLiveAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e) when (e is ApiException || e is TimeoutException || e is TaskCanceledException)
            {
                return new CachedClanWarLeagueGroup(tag, e, await clansCacheBase.ClanWarLeagueGroupTimeToLiveAsync(e).ConfigureAwait(false));
            }
        }

        public string Tag { get; internal set; }

        public DateTime Season { get; internal set; }

        public GroupState? State { get; internal set; }

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
