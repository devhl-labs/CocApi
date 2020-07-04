using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace devhl.CocApi.Models.Village
{
    public class LegendLeagueStatistics
    {
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
    }
}
