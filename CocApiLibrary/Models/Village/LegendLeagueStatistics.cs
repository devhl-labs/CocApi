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
        public Village? Village { get; internal set; }

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

        //[JsonProperty]
        //public IList<LegendLeagueResult> Results { get; internal set; } = new List<LegendLeagueResult>();

        public void Initialize(CocApi cocApi)
        {
            //if (PreviousSeason != null)
                //Results.Add(PreviousSeason);

            if (BestVersusSeason != null)
            {
                BestVersusSeason.Village = VillageType.BuilderBase;

                //Results.Add(BestVersusSeason);                
            }

            if (CurrentVersusSeason != null)
            {
                CurrentVersusSeason.Village = VillageType.BuilderBase;

                //Results.Add(CurrentVersusSeason);
            }

            //if (CurrentSeason != null)
            //    Results.Add(CurrentSeason);


            if (PreviousVersusSeason != null)
            {
                PreviousVersusSeason.Village = VillageType.BuilderBase;

                //Results.Add(PreviousVersusSeason);
            }

            //if (BestSeason != null)
                //Results.Add(BestSeason);            
        }
    }
}
