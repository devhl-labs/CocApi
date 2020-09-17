using System;
using CocApi.Model;
using CocApi.Client;
using System.Threading.Tasks;
using CocApi.Api;
using System.Threading;

namespace CocApi.Cache.Models
{
    public class CachedClan : CachedItem<Clan>
    {
        internal static async Task<CachedClan> FromClanResponseAsync(string tag, ClansClientBase clansCacheBase, ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<Clan> apiResponse = await clansApi.GetClanResponseAsync(tag, cancellationToken).ConfigureAwait(false);

                return new CachedClan(apiResponse, clansCacheBase.ClanTimeToLive(apiResponse));
            }
            catch (Exception e) when (e is ApiException || e is TimeoutException)
            {
                return new CachedClan(tag, e, clansCacheBase.ClanTimeToLive(e));
            }
        }

        public string Tag { get; internal set; }

        public bool Download { get; internal set; }

        public bool DownloadMembers { get; internal set; }

        public bool DownloadCurrentWar { get; internal set; }

        public bool DownloadCwl { get; internal set; }

        public bool IsWarLogPublic { get; internal set; }

        private CachedClan(ApiResponse<Clan> response, TimeSpan localExpiration) : base (response, localExpiration)
        {
            Tag = response.Data.Tag;

            IsWarLogPublic = response.Data.IsWarLogPublic;
        }

        private CachedClan(string tag, Exception exception, TimeSpan localExpiration) : base(exception, localExpiration)
        {
            Tag = tag;
        }

        public CachedClan(string tag)
        {
            Tag = tag;
        }

        public void UpdateFrom(CachedClan fetched)
        {
            if (ServerExpiration > fetched.ServerExpiration)
                return;

            IsWarLogPublic = fetched.IsWarLogPublic;

            base.UpdateFrom(fetched);
        }

    }
}
