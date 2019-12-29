using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.Clan
{
    public class TopClan : IClan
    {
        [JsonProperty("tag")]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty("location")]
        public Location? Location { get; internal set; }

        [JsonProperty("badgeUrls")]
        public BadgeUrl? BadgeUrl { get; internal set; }

        [JsonProperty("clanLevel")]
        public int ClanLevel { get; internal set; }

        [JsonProperty("members")]
        public int VillageCount { get; internal set; }

        [JsonProperty("rank")]
        public int Rank { get; internal set; }

        [JsonProperty("previousRank")]
        public int PreviousRank { get; internal set; }

        public void Initialize()
        {
        }
    }
}
