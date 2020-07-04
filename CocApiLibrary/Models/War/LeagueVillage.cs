﻿using devhl.CocApi.Models.Village;

using Newtonsoft.Json;
namespace devhl.CocApi.Models.War
{
    public class LeagueVillage : IVillage
    {
        [JsonProperty("Tag")]
        public string VillageTag { get; private set; } = string.Empty;


        [JsonProperty]
        public string Name { get; private set; } = string.Empty;


        [JsonProperty]
        public string ClanTag { get; internal set; } = string.Empty;


        [JsonProperty]
        public int TownhallLevel { get; private set; }

        public override string ToString() => $"{VillageTag} {Name} {TownhallLevel}";
    }
}
