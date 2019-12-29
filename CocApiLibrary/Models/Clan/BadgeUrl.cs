﻿using Newtonsoft.Json;
using System.Linq;

namespace devhl.CocApi.Models.Clan
{
    public class BadgeUrl
    {
        [JsonProperty]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Small { get; internal set; } = string.Empty;
        
        [JsonProperty]
        public string Large { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Medium { get; internal set; } = string.Empty;
    }
}
