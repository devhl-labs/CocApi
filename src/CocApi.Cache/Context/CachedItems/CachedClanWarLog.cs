using System;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Context.CachedItems
{
    public class CachedClanWarLog : CachedItem<ClanWarLog>
    {
        internal static async Task<CachedClanWarLog> FromClanWarLogResponseAsync(string tag, TimeToLiveProvider ttl, ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<ClanWarLog> apiResponse = await clansApi.FetchClanWarLogResponseAsync(tag, cancellationToken: cancellationToken);

                return new CachedClanWarLog(apiResponse, await ttl.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                cancellationToken?.ThrowIfCancellationRequested();

                return new CachedClanWarLog(await ttl.TimeToLiveOrDefaultAsync<ClanWarLog>(e).ConfigureAwait(false));
            }
        }

        internal static bool HasUpdated(CachedClanWarLog stored, CachedClanWarLog fetched)
        {
            if (stored.ExpiresAt > fetched.ExpiresAt)
                return false;

            if (fetched.Content == null)
                return false;

            return !fetched.Content.Equals(stored.Content);
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
