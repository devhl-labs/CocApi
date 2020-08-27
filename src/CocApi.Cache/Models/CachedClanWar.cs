using System;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedClanWar : CachedItem<ClanWar>
    {
        public string Tag { get; internal set; }

        public ClanWar.StateEnum? State { get; internal set; }

        public DateTime PreparationStartTime { get; internal set; }

        public ClanWar.TypeEnum Type { get; internal set; }

#nullable disable

        public CachedClanWar()
        {

        }

#nullable enable

        internal new void UpdateFrom(ApiResponse<ClanWar> responseItem, TimeSpan localExpiration)
        {
            base.UpdateFrom(responseItem, localExpiration);

            State = responseItem?.Data.State;

            PreparationStartTime = responseItem?.Data.PreparationStartTime ?? PreparationStartTime;
        }

        internal new void UpdateFrom(ApiException apiException, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiException, localExpiration);
        }
    }
}
