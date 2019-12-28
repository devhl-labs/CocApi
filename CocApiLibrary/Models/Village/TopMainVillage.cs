using System;
using System.Collections.Generic;
using System.Text;
////System.Text.Json.Serialization
using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class TopMainVillage :TopVillage
    {
        [JsonProperty("trophies")]
        public long Trophies { get; set; }

        [JsonProperty("attackWins")]
        public long AttackWins { get; set; }

        [JsonProperty("defenseWins")]
        public long DefenseWins { get; set; }

        [JsonProperty("league")]
        public VillageLeagueApiModel? League { get; set; }
    }
}
