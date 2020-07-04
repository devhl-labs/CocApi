using System;
using Newtonsoft.Json;
using devhl.CocApi.Converters;

namespace devhl.CocApi.Models.Village
{
    public class LegendLeagueResult
    {
        [JsonProperty]
        public int Trophies { get; internal set; }

        [JsonProperty]
        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime Id { get; internal set; }

        [JsonProperty]
        public int? Rank { get; internal set; }
    }
}
