using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary
{
    public class LegendLeagueStatisticsAPIModel
    {
        public string Tag { get; set; } = string.Empty;

        public int LegendTrophies { get; set; }

        public LegendLeagueResultAPIModel? CurrentSeason { get; set; }

        public LegendLeagueResultAPIModel? PreviousSeason { get; set; }

        public LegendLeagueResultAPIModel? BestSeason { get; set; }

        public LegendLeagueResultAPIModel? PreviousVersusSeason { get; set; }


    }
}
