using devhl.CocApi.Converters;
using System;
////System.Text.Json.Serialization
using static devhl.CocApi.Enums;
using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class WarLogEntryModel
    {
        //[JsonConverter(typeof(ResultConverter))]
        public Result Result { get; set; }

        public int TeamSize { get; set; }

        public WarClanApiModel? Clan { get; set; }

        public WarClanApiModel? Opponent { get; set; }

        [JsonProperty("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUtc { get; set; }

    }
}