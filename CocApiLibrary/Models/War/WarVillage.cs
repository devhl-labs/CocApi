using devhl.CocApi.Models.Village;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class WarVillage : IVillage, IWarVillage
    {
        [JsonProperty("Tag")]
        public string VillageTag { get; private set; } = string.Empty;

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;

        [JsonProperty]
        public string ClanTag { get; internal set; } = string.Empty;










        [JsonProperty]
        public int TownHallLevel { get; private set; }

        [JsonProperty("mapPosition")]
        public int RosterPosition { get; private set; }

        [JsonProperty]
        public int MapPosition { get; internal set; }


        [JsonProperty]
        public IList<Attack>? Attacks { get; internal set; }

        [JsonProperty]
        public string WarKey { get; internal set; } = string.Empty;

        public override string ToString() => $"{VillageTag} {Name}";
    }
}
