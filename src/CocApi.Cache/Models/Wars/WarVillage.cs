using CocApi.Cache.Models.Villages;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace CocApi.Cache.Models.Wars
{
    public class WarVillage : IVillage, IWarVillage
    {
        [JsonProperty("Tag")]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public int TownHallLevel { get; internal set; }

        [JsonProperty("mapPosition")]
        public int RosterPosition { get; internal set; }

        [JsonProperty]
        public int MapPosition { get; internal set; }


        [JsonProperty]
        public IList<Attack>? Attacks { get; internal set; }

        public override string ToString() => $"{VillageTag} {Name}";
    }
}
