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

                return new CachedClan(tag, apiResponse, await clansCacheBase.ClanTimeToLiveAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e) 
            {
                return new CachedClan(tag, await clansCacheBase.ClanTimeToLiveAsync(e).ConfigureAwait(false));
            }
        }

        public string Tag { get; internal set; }

        public bool DownloadMembers { get; internal set; }

        public bool DownloadCurrentWar { get; internal set; }

        public bool DownloadCwl { get; internal set; }

        public bool? IsWarLogPublic { get; internal set; }

        private CachedClan(string tag, ApiResponse<Clan> response, TimeSpan localExpiration) : base (response, localExpiration)
        {
            Tag = tag;
            
            if (response.Data != null)            
                IsWarLogPublic = response.Data.IsWarLogPublic;                        
        }

        private CachedClan(string tag, TimeSpan localExpiration) : base(localExpiration)
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
