using System;
using System.Text.Json.Serialization;

using devhl.CocApi.Converters;
using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.Village
{
    public class LegendLeagueResultApiModel
    {
        public string VillageTag { get; set; } = string.Empty;

        public int Trophies { get; set; }

        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime Id { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);

        public int? Rank { get; set; }

        public VillageType Village { get; set; } = VillageType.Home;
    }
}
