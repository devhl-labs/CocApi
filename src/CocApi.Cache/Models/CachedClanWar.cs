using System;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWar : CachedItem<ClanWar>
    {
        internal static async Task<CachedClanWar> FromCurrentWarResponseAsync(string tag, ClansClientBase clansCacheBase, ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<ClanWar> apiResponse = await clansApi.GetCurrentWarResponseAsync(tag, cancellationToken);

                CachedClanWar result = new CachedClanWar(tag, apiResponse, await clansCacheBase.ClanWarTimeToLiveAsync(apiResponse).ConfigureAwait(false));

                //result.Type = result.Data.WarType;

                return result;
            }
            catch (Exception e) when (e is ApiException || e is TimeoutException)
            {
                return new CachedClanWar(tag, e, await clansCacheBase.ClanWarTimeToLiveAsync(e).ConfigureAwait(false));
            }
        }

        internal static bool IsNewWar(CachedClanWar stored, CachedClanWar fetched)
        {
            if (fetched.Data == null || fetched.Data.State == WarState.NotInWar)
                return false;

            if (stored.Data == null)
                return true;

            if (stored.Data.PreparationStartTime == fetched.Data.PreparationStartTime)
                return false;

            return true;
        }

        public string Tag { get; internal set; }

        public WarState? State { get; internal set; }

        public DateTime PreparationStartTime { get; internal set; }

        public WarType Type { get; internal set; }

        internal CachedClanWar(string tag)
        {
            Tag = tag;
        }

        private CachedClanWar(string clanTag, ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiResponse, localExpiration);

            State = apiResponse?.Data.State;

            PreparationStartTime = apiResponse?.Data.PreparationStartTime ?? PreparationStartTime;

            Tag = clanTag;
        }

        private CachedClanWar(string clanTag, Exception exception, TimeSpan localExpiration)
        {
            base.UpdateFrom(exception, localExpiration);

            Tag = clanTag;
        }

        internal void UpdateFrom(CachedClanWar fetched)
        {
            if (ServerExpiration > fetched.ServerExpiration)
                return;

            base.UpdateFrom(fetched);

            State = fetched.State;

            PreparationStartTime = fetched.PreparationStartTime;

            //Type = fetched.Type;  //only do this at the beginning of war
        }
    }
}
