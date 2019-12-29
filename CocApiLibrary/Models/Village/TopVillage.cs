using devhl.CocApi.Models.Clan;
using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class TopVillage : IVillage, IInitialize
    {
        [JsonProperty("tag")]
        public string VillageTag { get;  } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; } = string.Empty;

        [JsonProperty("expLevel")]
        public long ExpLevel { get; }

        [JsonProperty("rank")]
        public long Rank { get; }

        [JsonProperty("previousRank")]
        public long PreviousRank { get; }

        [JsonProperty("clan")]
        public SimpleClan? Clan { get; }


        [JsonProperty]
        public string ClanTag { get; private set; } = string.Empty;

        public void Initialize()
        {
            if (Clan != null)
            {
                ClanTag = Clan.ClanTag;
            }
        }
    }
}
