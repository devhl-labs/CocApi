using devhl.CocApi.Models.Clan;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.Village
{
    public class TopVillage : IVillageApiModel, IInitialize
    {
        [JsonPropertyName("tag")]
        public string VillageTag { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("expLevel")]
        public long ExpLevel { get; set; }

        [JsonPropertyName("rank")]
        public long Rank { get; set; }

        [JsonPropertyName("previousRank")]
        public long PreviousRank { get; set; }

        [JsonPropertyName("clan")]
        public SimpleClanApiModel? Clan { get; set; }



        public string ClanTag { get; set; } = string.Empty;

        public void Initialize()
        {
            if (Clan != null)
            {
                ClanTag = Clan.ClanTag;
            }
        }
    }
}
