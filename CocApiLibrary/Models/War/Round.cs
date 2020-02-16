using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace devhl.CocApi.Models.War
{
    public class Round
    {
        [JsonProperty]
        public string RoundKey { get; internal set; } = string.Empty;

        [JsonProperty]
        public string GroupKey { get; internal set; } = string.Empty;

        [JsonProperty]
        public List<string>? WarTags { get; private set; }
    }
}
