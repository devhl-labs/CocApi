using devhl.CocApi.Converters;
using System;
using System.Text.Json.Serialization;
using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models
{
    public class WarLogEntryModel
    {
        [JsonConverter(typeof(ResultConverter))]
        public Result Result { get; set; }

        public int TeamSize { get; set; }

        public WarClanApiModel? Clan { get; set; }

        public WarClanApiModel? Opponent { get; set; }

        [JsonPropertyName("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUtc { get; set; }

    }
}