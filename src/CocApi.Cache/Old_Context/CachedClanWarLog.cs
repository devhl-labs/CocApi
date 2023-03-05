using System;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.IBaseApis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace CocApi.Cache.Models
{
    public class CachedClanWarLog : CachedItem<ClanWarLog>
    {
        internal static async Task<CachedClanWarLog> FromClanWarLogResponseAsync(string tag, TimeToLiveProvider ttl, IClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<ClanWarLog> apiResponse = await clansApi.FetchClanWarLogResponseAsync(tag, cancellationToken: cancellationToken);

                return new CachedClanWarLog(tag, apiResponse, await ttl.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                cancellationToken?.ThrowIfCancellationRequested();

                return new CachedClanWarLog(tag, await ttl.TimeToLiveOrDefaultAsync<ClanWarLog>(e).ConfigureAwait(false));
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

        private CachedClanWarLog(string tag, TimeSpan localExpiration)
        {
            Tag = tag;

            UpdateFrom(localExpiration);
        }

        internal void UpdateFrom(CachedClanWarLog fetched)
        {
            if (ServerExpiration > fetched.ServerExpiration)
                return;

            base.UpdateFrom(fetched);
        }
    }
}
