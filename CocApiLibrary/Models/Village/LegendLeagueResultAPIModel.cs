using CocApiLibrary.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CocApiLibrary
{
    public class LegendLeagueResultAPIModel
    {
        public int Trophies { get; set; }

        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime? ID { get; set; }

        public int? Rank { get; set; }

    }
}
