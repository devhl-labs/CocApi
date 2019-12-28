using devhl.CocApi.Models.Clan;
using System;
using System.Collections.Generic;
using System.Text;
////System.Text.Json.Serialization
using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class TopVillage : IVillageApiModel, IInitialize
    {
        [JsonProperty("tag")]
        public string VillageTag { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("expLevel")]
        public long ExpLevel { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("previousRank")]
        public long PreviousRank { get; set; }

        [JsonProperty("clan")]
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
