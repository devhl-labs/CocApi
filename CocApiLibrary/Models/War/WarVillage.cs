using devhl.CocApi.Models.Village;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class WarVillage : IVillage
    {
        [JsonProperty("Tag")]
        public string VillageTag { get; private set; } = string.Empty;

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;

        [JsonProperty]
        public string ClanTag { get; internal set; } = string.Empty;








        [JsonProperty]
        public string WarVillageId { get; internal set; } = string.Empty;

        [JsonProperty]
        public int TownhallLevel { get; private set; }

        [JsonProperty]
        public int MapPosition { get; private set; }


        [JsonProperty]
        public string WarClanId { get; internal set; } = string.Empty;


        [JsonProperty]
        public IList<Attack>? Attacks { get; internal set; }

        [JsonProperty]
        public string WarId { get; internal set; } = string.Empty;

        public void Initialize()
        {            
            if (!string.IsNullOrEmpty(WarId) && !string.IsNullOrEmpty(VillageTag))
            {
                WarVillageId = $"{WarId};{VillageTag}";
            }
        }

        public override string ToString() => $"{VillageTag} {Name}";
    }
}
