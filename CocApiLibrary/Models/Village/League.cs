﻿using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class League
    {
        public static string Url() => "leagues?limit=500";
        

        [JsonProperty]
        public int Id { get; internal set; }

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty("iconUrls")]
        public LeagueIcon? LeagueIcon { get; internal set; }

        public override string ToString() => Name;
    }
}
