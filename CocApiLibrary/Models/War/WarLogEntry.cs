using devhl.CocApi.Converters;
using System;

using static devhl.CocApi.Enums;
using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class WarLogEntry
    {

        [JsonProperty]
        public Result Result { get; }

        [JsonProperty]
        public int TeamSize { get; }

        [JsonProperty]
        public WarClan? Clan { get; }

        [JsonProperty]
        public WarClan? Opponent { get; }

        [JsonProperty("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUtc { get; }

    }
}