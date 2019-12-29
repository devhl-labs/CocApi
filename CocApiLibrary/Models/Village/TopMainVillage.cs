using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class TopMainVillage :TopVillage
    {
        [JsonProperty("trophies")]
        public long Trophies { get; }

        [JsonProperty("attackWins")]
        public long AttackWins { get; }

        [JsonProperty("defenseWins")]
        public long DefenseWins { get; }

        [JsonProperty("league")]
        public League? League { get; }
    }
}
