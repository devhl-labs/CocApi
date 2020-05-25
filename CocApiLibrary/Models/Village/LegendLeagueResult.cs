using System;
using Newtonsoft.Json;
using devhl.CocApi.Converters;

namespace devhl.CocApi.Models.Village
{
    public class LegendLeagueResult
    {
        [JsonProperty]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public int Trophies { get; internal set; }

        [JsonProperty]
        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime Id { get; internal set; }

        [JsonProperty]
        public int? Rank { get; internal set; }

        [JsonProperty]
        public VillageType VillageType { get; internal set; } = VillageType.Home;
    }
}
