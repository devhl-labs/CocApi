using devhl.CocApi.Models.Clan;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static devhl.CocApi.Enums;
using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class WarClan : IClan
    {
        [JsonProperty("Tag")]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; } = string.Empty;


        [JsonProperty]
        public ClanBadgeUrl? BadgeUrls { get; }


        [JsonProperty]
        public int ClanLevel { get; }

        [JsonProperty]
        public string WarId { get; internal set; } = string.Empty;

        [JsonProperty("members")]
        public IEnumerable<WarVillage>? Villages { get; }

        [JsonProperty("attacks")]
        public int AttackCount { get; }

        [JsonProperty]
        public int DefenseCount { get; internal set; }

        [JsonProperty]
        public int Stars { get; }

        [JsonProperty]
        public decimal DestructionPercentage { get; }

        [JsonProperty]
        public string WarClanId { get; private set; } = string.Empty;

        [JsonProperty]
        public Result Result { get; internal set; }

        public void Initialize()
        {
            if (BadgeUrls != null) BadgeUrls.Initialize();

            if (!string.IsNullOrEmpty(WarId) && !string.IsNullOrEmpty(ClanTag))
            {
                WarClanId = $"{WarId};{ClanTag}";
            }

            if (!string.IsNullOrEmpty(ClanTag))
            {
                foreach (var village in Villages.EmptyIfNull())
                {
                    village.ClanTag = ClanTag;
                }
            }
        }
    }
}
