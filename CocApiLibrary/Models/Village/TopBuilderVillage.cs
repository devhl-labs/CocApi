using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace devhl.CocApi.Models.Village
{
    public class TopBuilderVillage : TopVillage
    {
        [JsonProperty("versusTrophies")]
        public long VersusTrophies { get;  }

        [JsonProperty("versusBattleWins")]
        public int VersusBattleWins { get; }
    }
}
