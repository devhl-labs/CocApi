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
        public string Name { get; private set; } = string.Empty;


        [JsonProperty]
        public BadgeUrl? BadgeUrl { get; private set; }


        [JsonProperty]
        public int ClanLevel { get; private set; }

        [JsonProperty]
        public string WarId { get; internal set; } = string.Empty;

        [JsonProperty("members")]
        public IEnumerable<WarVillage>? Villages { get; private set; }

        [JsonProperty("attacks")]
        public int AttackCount { get; }

        [JsonProperty]
        public int DefenseCount { get; internal set; }

        [JsonProperty]
        public int Stars { get; private set; }

        [JsonProperty]
        public decimal DestructionPercentage { get; private set; }

        [JsonProperty]
        public string WarClanId { get; private set; } = string.Empty;

        [JsonProperty]
        public Result Result { get; internal set; }

        public void Initialize()
        {
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
