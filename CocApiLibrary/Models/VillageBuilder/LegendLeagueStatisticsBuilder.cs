using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace devhl.CocApi.Models.Village
{
    public class LegendLeagueStatisticsBuilder
    {
        public string VillageTag { get; set; } = string.Empty;

        public Village? Village { get; set; }

        public int LegendTrophies { get; set; }

        public LegendLeagueResultBuilder? BestSeason { get; set; }

        public LegendLeagueResultBuilder? PreviousVersusSeason { get; set; }

        public LegendLeagueResultBuilder? CurrentSeason { get; set; }

        public LegendLeagueResultBuilder? CurrentVersusSeason { get; set; }

        public LegendLeagueResultBuilder? BestVersusSeason { get; set; }

        public LegendLeagueResultBuilder? PreviousSeason { get; set; }

        internal LegendLeagueStatistics Build()
        {
            LegendLeagueStatistics legendLeagueStatistics = new LegendLeagueStatistics
            {
                VillageTag = VillageTag,
                LegendTrophies = LegendTrophies,
                BestSeason = BestSeason?.Build(),
                PreviousVersusSeason = PreviousVersusSeason?.Build(),
                CurrentSeason = CurrentSeason?.Build(),
                CurrentVersusSeason = CurrentVersusSeason?.Build(),
                BestVersusSeason = BestVersusSeason?.Build(),
                PreviousSeason = PreviousSeason?.Build()
            };

            return legendLeagueStatistics;
        }
    }
}
