using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace devhl.CocApi.Models.Village
{
    public class TopBuilderVillage : TopVillage
    {
        public static string Url(int? locationId)
        {
            string location = "global";

            if (locationId != null)
                location = locationId.ToString();

            return $"{location}/rankings/players-versus";
        }

        [JsonProperty("versusTrophies")]
        public long VersusTrophies { get; private set; }

        [JsonProperty("versusBattleWins")]
        public int VersusBattleWins { get; private set; }
    }
}
