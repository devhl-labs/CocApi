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
        public string VillageTag { get; }

        [JsonProperty]
        public string Name { get; } = string.Empty;

        [JsonProperty]
        public string ClanTag { get; internal set; } = string.Empty;








        [JsonProperty]
        public string WarVillageId { get; internal set; } = string.Empty;

        [JsonProperty]
        public int TownhallLevel { get; }

        [JsonProperty]
        public int MapPosition { get; }


        [JsonProperty]
        public string WarClanId { get; internal set; } = string.Empty;


        [JsonProperty]
        public IList<Attack>? Attacks { get; }

        [JsonProperty]
        public string WarId { get; internal set; } = string.Empty;

        public void Initialize()
        {            
            if (!string.IsNullOrEmpty(WarId) && !string.IsNullOrEmpty(VillageTag))
            {
                WarVillageId = $"{WarId};{VillageTag}";
            }
        }
    }
}
