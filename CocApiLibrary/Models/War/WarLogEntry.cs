using devhl.CocApi.Converters;
using System;

//using static devhl.CocApi.Enums;
using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class WarLogEntry
    {

        [JsonProperty]
        public Result Result { get; private set; }

        [JsonProperty]
        public int TeamSize { get; private set; }

        [JsonProperty]
        public WarClan? Clan { get; private set; }

        [JsonProperty]
        public WarClan? Opponent { get; private set; }

        [JsonProperty("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUtc { get; private set; }

        public override string ToString() => EndTimeUtc.ToString();
    }
}