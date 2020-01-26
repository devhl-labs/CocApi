using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace devhl.CocApi.Models.Village
{
    public class LegendLeagueStatistics : IInitialize
    {
        [JsonProperty]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public Village? Village { get; private set; }

        [JsonProperty]
        public int LegendTrophies { get; private set; }

        [JsonProperty]
        public LegendLeagueResult? BestSeason { get; private set; }


        [JsonProperty]
        public LegendLeagueResult? PreviousVersusSeason { get; private set; }


        [JsonProperty]
        public LegendLeagueResult? CurrentSeason { get; private set; }


        [JsonProperty]
        public LegendLeagueResult? CurrentVersusSeason { get; private set; }


        [JsonProperty]
        public LegendLeagueResult? BestVersusSeason { get; private set; }


        [JsonProperty]
        public LegendLeagueResult? PreviousSeason { get; private set; } 

        [JsonProperty]
        public IList<LegendLeagueResult> Results { get; private set; } = new List<LegendLeagueResult>();

        public void Initialize()
        {
            if (PreviousSeason != null)
            {
                if (!Results.Any(l => l.Id == PreviousSeason.Id))
                {
                    Results.Add(PreviousSeason);
                }
            }

            if (BestVersusSeason != null)
            {
                BestVersusSeason.Village = VillageType.BuilderBase;

                if (!Results.Any(l => l.Id == BestVersusSeason.Id))
                {
                    Results.Add(BestVersusSeason);
                }
            }

            if (CurrentVersusSeason != null)
            {
                CurrentVersusSeason.Village = VillageType.BuilderBase;

                if (!Results.Any(l => l.Id == CurrentVersusSeason.Id))
                {
                    Results.Add(CurrentVersusSeason);
                }
            }

            if (CurrentSeason != null)
            {
                if (!Results.Any(l => l.Id == CurrentSeason.Id))
                {
                    Results.Add(CurrentSeason);
                }
            }

            if (PreviousVersusSeason != null)
            {
                PreviousVersusSeason.Village = VillageType.BuilderBase;

                if (!Results.Any(l => l.Id == PreviousVersusSeason.Id))
                {
                    Results.Add(PreviousVersusSeason);
                }
            }

            if (BestSeason != null)
            {
                if (!Results.Any(l => l.Id == BestSeason.Id))
                {
                    Results.Add(BestSeason);
                }
            }
        }
    }
}
