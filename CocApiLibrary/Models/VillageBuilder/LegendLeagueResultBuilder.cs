using System;
using Newtonsoft.Json;
using devhl.CocApi.Converters;

namespace devhl.CocApi.Models.Village
{
    public class LegendLeagueResultBuilder
    {

        public string VillageTag { get; set; } = string.Empty;

        public int Trophies { get; set; }

        public DateTime Id { get; set; }

        public int? Rank { get; set; }

        public VillageType Village { get; set; } = VillageType.Home;

        internal LegendLeagueResult Build()
        {
            LegendLeagueResult legendLeagueResult = new LegendLeagueResult
            {
                VillageTag = VillageTag,
                Trophies = Trophies,
                Id = Id,
                Rank = Rank,
                VillageType = Village
            };

            return legendLeagueResult;
        }
    }
}
