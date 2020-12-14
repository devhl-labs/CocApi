using System;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWarLog : CachedItem<ClanWarLog>
    {
        internal static async Task<CachedClanWarLog> FromClanWarLogResponseAsync(string token, string tag, ClansClientBase clansCacheBase, ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<ClanWarLog> apiResponse = await clansApi.GetClanWarLogResponseAsync(token, tag, cancellationToken: cancellationToken);

                return new CachedClanWarLog(tag, apiResponse, await clansCacheBase.ClanWarLogTimeToLiveAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e) when (e is ApiException || e is TimeoutException || e is TaskCanceledException || e is CachedHttpRequestException)
            {
                return new CachedClanWarLog(tag, e, await clansCacheBase.ClanWarLogTimeToLiveAsync(e).ConfigureAwait(false));
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

        private CachedClanWarLog(string tag, Exception exception, TimeSpan localExpiration)
        {
            Tag = tag;

            UpdateFrom(exception, localExpiration);
        }

        internal void UpdateFrom(CachedClanWarLog fetched)
        {
            if (ServerExpiration > fetched.ServerExpiration)
                return;

            base.UpdateFrom(fetched);
        }
    }
}
