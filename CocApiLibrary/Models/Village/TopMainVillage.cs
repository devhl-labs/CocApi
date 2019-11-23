using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.Village
{
    public class TopMainVillage :TopVillage
    {
        [JsonPropertyName("trophies")]
        public long Trophies { get; set; }

        [JsonPropertyName("attackWins")]
        public long AttackWins { get; set; }

        [JsonPropertyName("defenseWins")]
        public long DefenseWins { get; set; }

        [JsonPropertyName("league")]
        public VillageLeagueApiModel? League { get; set; }
    }
}
