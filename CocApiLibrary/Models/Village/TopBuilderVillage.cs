using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.Village
{
    public class TopBuilderVillage : TopVillage
    {
        [JsonPropertyName("versusTrophies")]
        public long VersusTrophies { get; set; }

        [JsonPropertyName("versusBattleWins")]
        public int VersusBattleWins { get; set; }
    }
}
