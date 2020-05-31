using devhl.CocApi.Models.War;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi.Models.Cache
{
    public class CachedClanWar
    {
        public string ClanTag { get; set; } = string.Empty;

        public DateTime PrepStartTime { get; set; }

        public string Json { get; set; } = string.Empty;

        public bool IsDownloadable { get; set; }

        public DateTime UpdatesAt { get; set; } = DateTime.UtcNow;

        public WarState WarState { get; set; }

        public CachedClanWar()
        {

        }

        public CachedClanWar(string clanTag, CurrentWar currentWar)
        {
            ClanTag = clanTag;

            PrepStartTime = currentWar.PreparationStartTimeUtc;

            Json = currentWar.ToJson();

            IsDownloadable = true;

            WarState = currentWar.State;

            UpdatesAt = currentWar.ServerExpirationUtc;
        }

        public CurrentWar ToCurrentWar() => JsonConvert.DeserializeObject<CurrentWar>(Json);
    }
}
