using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.Clan
{
    public class TopClan : IClanApiModel
    {
        [JsonPropertyName("tag")]
        public string ClanTag { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public LocationApiModel? Location { get; set; }

        [JsonPropertyName("badgeUrls")]
        public ClanBadgeUrlApiModel? BadgeUrls { get; set; }

        [JsonPropertyName("clanLevel")]
        public int ClanLevel { get; set; }

        [JsonPropertyName("members")]
        public int VillageCount { get; set; }

        //[JsonPropertyName("clanPoints")]
        //public int ClanPoints { get; set; }

        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("previousRank")]
        public int PreviousRank { get; set; }
    }
}
