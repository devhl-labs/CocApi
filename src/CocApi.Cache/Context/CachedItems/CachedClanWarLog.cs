﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Context.CachedItems
{
    public class CachedClanWarLog : CachedItem<ClanWarLog>
    {
        internal static async Task<CachedClanWarLog> FromClanWarLogResponseAsync(string tag, ClansClientBase clansCacheBase, ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<ClanWarLog> apiResponse = await clansApi.FetchClanWarLogResponseAsync(tag, cancellationToken: cancellationToken);

                return new CachedClanWarLog(apiResponse, await clansCacheBase.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                return new CachedClanWarLog(await clansCacheBase.TimeToLiveOrDefaultAsync<ClanWarLog>(e).ConfigureAwait(false));
            }
        }

        public CachedClanWarLog()
        {

        }

        private CachedClanWarLog(ApiResponse<ClanWarLog> apiResponse, TimeSpan localExpiration)
        {
            UpdateFrom(apiResponse, localExpiration);
        }

        private CachedClanWarLog(TimeSpan localExpiration)
        {
            UpdateFrom(localExpiration);
        }

        internal void UpdateFrom(CachedClanWarLog fetched)
        {
            if (ExpiresAt > fetched.ExpiresAt)
                return;

            base.UpdateFrom(fetched);
        }
    }
}