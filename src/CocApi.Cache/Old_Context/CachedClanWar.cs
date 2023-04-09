using System;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.Apis;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace CocApi.Cache.Models
{
    public class CachedClanWar : CachedItem<ClanWar>
    {
        internal static async Task<CachedClanWar> FromCurrentWarResponseAsync(string tag, bool? realtime, TimeToLiveProvider ttl, ClansApi clansApi, CancellationToken? cancellationToken = default)
        {
            try
            {
                ApiResponse<ClanWar?> apiResponse = await clansApi.FetchCurrentWarResponseAsync(tag, realtime, cancellationToken);

                return new CachedClanWar(tag, apiResponse, await ttl.TimeToLiveOrDefaultAsync(apiResponse).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                cancellationToken?.ThrowIfCancellationRequested();

                return new CachedClanWar(tag, await ttl.TimeToLiveOrDefaultAsync<ClanWar>(e).ConfigureAwait(false));
            }
        }

        internal static bool IsNewWar(CachedClanWar stored, CachedClanWar fetched)
        {
            if (fetched.Data == null || fetched.Data.State == Rest.Models.WarState.NotInWar)
                return false;

            if (stored.Data == null)
                return true;

            if (stored.Data.PreparationStartTime == fetched.Data.PreparationStartTime)
                return false;

            return true;
        }

        public string Tag { get; internal set; }

        public Rest.Models.WarState? State { get; internal set; }

        public DateTime PreparationStartTime { get; internal set; }

        public Rest.Models.WarType Type { get; internal set; }

        internal CachedClanWar(string tag)
        {
            Tag = tag;
        }

        private CachedClanWar(string clanTag, ApiResponse<ClanWar?> apiResponse, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiResponse, localExpiration);

            Tag = clanTag;

            if (apiResponse.Content != null)
            {
                State = apiResponse.Content.State;

                PreparationStartTime = apiResponse.Content.PreparationStartTime;
            }
        }

        private CachedClanWar(string clanTag, TimeSpan localExpiration)
        {
            base.UpdateFrom(localExpiration);

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
