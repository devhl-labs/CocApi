using CocApiLibrary.Converters;
using System;
using System.Text.Json.Serialization;
using static CocApiStandardLibrary.Enums;

namespace CocApiStandardLibrary.Models
{
    public class WarLogEntryModel
    {
        [JsonConverter(typeof(ResultConverter))]
        public Result Result { get; set; }

        public int TeamSize { get; set; }

        public WarClanAPIModel? Clan { get; set; }

        public WarClanAPIModel? Opponent { get; set; }

        [JsonPropertyName("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUTC { get; set; }







        internal void Process()
        {
            Clan?.Process();
            Opponent?.Process();
        }
    }
}
