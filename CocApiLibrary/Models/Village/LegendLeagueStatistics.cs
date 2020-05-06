using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace devhl.CocApi.Models.Village
{
    public class LegendLeagueStatistics : IInitialize
    {
        [JsonProperty]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public int LegendTrophies { get; internal set; }

        [JsonProperty]
        public LegendLeagueResult? BestSeason { get; internal set; }

        [JsonProperty]
        public LegendLeagueResult? PreviousVersusSeason { get; internal set; }

        [JsonProperty]
        public LegendLeagueResult? CurrentSeason { get; internal set; }

        [JsonProperty]
        public LegendLeagueResult? CurrentVersusSeason { get; internal set; }

        [JsonProperty]
        public LegendLeagueResult? BestVersusSeason { get; internal set; }

        [JsonProperty]
        public LegendLeagueResult? PreviousSeason { get; internal set; } 

        public void Initialize(CocApi cocApi)
        {
            if (BestVersusSeason != null)
                BestVersusSeason.VillageType = VillageType.BuilderBase;

            if (CurrentVersusSeason != null)
                CurrentVersusSeason.VillageType = VillageType.BuilderBase;

            if (PreviousVersusSeason != null)
                PreviousVersusSeason.VillageType = VillageType.BuilderBase;           
        }
    }
}
