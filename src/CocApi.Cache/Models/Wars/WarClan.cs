using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using CocApi.Cache.Converters;
using CocApi.Cache.Models.Clans;

namespace CocApi.Cache.Models.Wars
{
    public class WarClan : IClan, IWarClan, IInitialize
    {
        [JsonProperty("Tag")]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;


        [JsonProperty("badgeUrls")]
        public BadgeUrl? BadgeUrl { get; private set; }


        [JsonProperty]
        public int ClanLevel { get; private set; }

        [JsonProperty]
        public string WarKey { get; internal set; } = string.Empty;

        [JsonProperty("members")]
        public IEnumerable<WarVillage>? WarVillages { get; internal set; }

        [JsonProperty("attacks")]
        public int AttackCount { get; internal set; }

        [JsonProperty]
        public int DefenseCount { get; internal set; }

        [JsonProperty]
        public int Stars { get; internal set; }

        [JsonProperty]
        public decimal DestructionPercentage { get; internal set; }

        [JsonProperty]
        [JsonConverter(typeof(ResultConverter))]
        public Result Result { get; internal set; }

        public void Initialize(CocApiClient_old cocApi)
        {
            WarVillages = WarVillages.ToList().OrderBy(wv => wv.RosterPosition);

            foreach (var warVillage in WarVillages.EmptyIfNull())            
                warVillage.ClanTag = ClanTag;
            
        }

        public override string ToString() => $"{ClanTag} {Name}";
    }
}
