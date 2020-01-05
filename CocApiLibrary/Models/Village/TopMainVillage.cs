using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class TopMainVillage :TopVillage
    {
        [JsonProperty("trophies")]
        public long Trophies { get; private set; }

        [JsonProperty("attackWins")]
        public long AttackWins { get; private set; }

        [JsonProperty("defenseWins")]
        public long DefenseWins { get; private set; }

        [JsonProperty("league")]
        public League? League { get; private set; }
    }
}
