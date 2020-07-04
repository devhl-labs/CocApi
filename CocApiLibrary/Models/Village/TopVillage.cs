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
        public string VillageTag { get; private set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; private set; } = string.Empty;

        [JsonProperty("expLevel")]
        public long ExpLevel { get; private set; }

        [JsonProperty("rank")]
        public long Rank { get; private set; }

        [JsonProperty("previousRank")]
        public long PreviousRank { get; private set; }

        [JsonProperty("clan")]
        public SimpleClan? Clan { get; private set; }


        [JsonProperty]
        public string ClanTag { get; private set; } = string.Empty;

        public void Initialize(CocApi cocApi)
        {
            if (Clan != null)
            {
                ClanTag = Clan.ClanTag;
            }
        }

        public override string ToString() => $"{Rank} {VillageTag} {Name}";
    }
}
