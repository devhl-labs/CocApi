﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Context.CachedItems
{
    public class CachedClanWarLeagueGroup : CachedItem<ClanWarLeagueGroup>
    {
        internal static async Task<CachedClanWarLeagueGroup> FromClanWarLeagueGroupResponseAsync(string tag, ClansClientBase clansCacheBase, ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<ClanWarLeagueGroup> apiResponse = await clansApi.FetchClanWarLeagueGroupResponseAsync(tag, cancellationToken).ConfigureAwait(false);

                return new CachedClanWarLeagueGroup(apiResponse, await clansCacheBase.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                return new CachedClanWarLeagueGroup(await clansCacheBase.TimeToLiveOrDefaultAsync<ClanWarLeagueGroup>(e).ConfigureAwait(false));
            }
        }

        public DateTime? Season { get; internal set; }

        public GroupState? State { get; internal set; }

        public bool Added { get; internal set; }

        internal CachedClanWarLeagueGroup()
        {
        }

        private CachedClanWarLeagueGroup(ApiResponse<ClanWarLeagueGroup> apiResponse, TimeSpan localExpiration)
        {
            UpdateFrom(apiResponse, localExpiration);

            Season = apiResponse.Content?.Season;

            State = apiResponse.Content?.State;
        }

        private CachedClanWarLeagueGroup(TimeSpan localExpiration)
        {
            UpdateFrom(localExpiration);
        }

        internal void UpdateFrom(CachedClanWarLeagueGroup fetched)
        {
            if (ExpiresAt > fetched.ExpiresAt)
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