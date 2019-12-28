using devhl.CocApi.Models.Location;
using System;
using System.Collections.Generic;
using System.Text;
////System.Text.Json.Serialization
using Newtonsoft.Json;

namespace devhl.CocApi.Models.Clan
{
    public class TopClan : IClanApiModel
    {
        [JsonProperty("tag")]
        public string ClanTag { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("location")]
        public LocationApiModel? Location { get; set; }

        [JsonProperty("badgeUrls")]
        public ClanBadgeUrlApiModel? BadgeUrls { get; set; }

        [JsonProperty("clanLevel")]
        public int ClanLevel { get; set; }

        [JsonProperty("members")]
        public int VillageCount { get; set; }

        //[JsonProperty("clanPoints")]
        //public int ClanPoints { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("previousRank")]
        public int PreviousRank { get; set; }
    }
}
