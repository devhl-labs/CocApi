using System;
using Newtonsoft.Json;
using CocApi.Cache.Converters;

namespace CocApi.Cache.Models.Villages
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
