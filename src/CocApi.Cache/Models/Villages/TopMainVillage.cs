using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace CocApi.Cache.Models.Villages
{
    public class TopMainVillage :TopVillage
    {
        public static string Url(int? locationId)
        {
            string location = "global";

            if (locationId != null)
                location = locationId.ToString();

            return $"{location}/rankings/players";
        }

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
