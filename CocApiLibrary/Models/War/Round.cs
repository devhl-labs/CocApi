using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace devhl.CocApi.Models.War
{
    public class Round
    {
        [JsonProperty]
        public string RoundId { get; internal set; } = string.Empty;

        [JsonProperty]
        public string GroupId { get; internal set; } = string.Empty;

        [JsonProperty]
        public List<string>? WarTags { get; }
    }
}
