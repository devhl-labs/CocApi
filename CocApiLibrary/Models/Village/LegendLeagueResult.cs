using System;

using Newtonsoft.Json;

using devhl.CocApi.Converters;
//using static devhl.CocApi.Enums;

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
        public DateTime Id { get; internal set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);

        [JsonProperty]
        public int? Rank { get; internal set; }

        [JsonProperty]
        public VillageType Village { get; internal set; } = VillageType.Home;
    }
}
