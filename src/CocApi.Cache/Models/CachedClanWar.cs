using System;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWar : CachedItem<ClanWar>
    {
        public string Tag { get; set; }

        public ClanWar.StateEnum? State { get; set; }

        public DateTime PreparationStartTime { get; set; }

#nullable disable

        public CachedClanWar()
        {

        }

#nullable enable

        public CachedClanWar(CachedClan cachedClan, ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration) : base(apiResponse, localExpiration)
        {
            Tag = cachedClan.Tag;

            State = apiResponse.Data.State;

            PreparationStartTime = apiResponse.Data.PreparationStartTime;
        }

        public new void UpdateFrom(ApiResponse<ClanWar>? responseItem, TimeSpan localExpiration)
        {
            base.UpdateFrom(responseItem, localExpiration);

            State = responseItem?.Data.State;
        }
    }
}
