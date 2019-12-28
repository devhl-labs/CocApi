using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
////System.Text.Json.Serialization

namespace devhl.CocApi.Models.Village
{
    public class TopBuilderVillage : TopVillage
    {
        [JsonProperty("versusTrophies")]
        public long VersusTrophies { get; set; }

        [JsonProperty("versusBattleWins")]
        public int VersusBattleWins { get; set; }
    }
}
