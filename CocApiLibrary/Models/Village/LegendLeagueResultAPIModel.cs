using CocApiLibrary.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary
{
    public class LegendLeagueResultAPIModel
    {
        public string VillageTag { get; set; } = string.Empty;

        public int Trophies { get; set; }

        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime Id { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);

        public int? Rank { get; set; }

        public VillageType Village { get; set; } = VillageType.Home;
    }
}
