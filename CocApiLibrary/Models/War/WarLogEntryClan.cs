using devhl.CocApi.Models.Clan;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using devhl.CocApi.Converters;

namespace devhl.CocApi.Models.War
{
    public class WarLogEntryClan : IClan, IWarClan
    {

        [JsonProperty("tag")]
#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
        public string? ClanTag { get; internal set; }

        [JsonProperty]
        public string? Name { get; private set; }

#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        [JsonProperty("badgeUrls")]
        public BadgeUrl? BadgeUrl { get; private set; }

        [JsonProperty]
        public int ClanLevel { get; private set; }

        [JsonProperty]
        public int Stars { get; internal set; }

        [JsonProperty]
        public decimal DestructionPercentage { get; internal set; }

        [JsonProperty]
        [JsonConverter(typeof(ResultConverter))]
        public Result Result { get; internal set; }

        [JsonProperty]
        public int ExpEarned { get; private set; }

        [JsonProperty("attacks")]
        public int? AttackCount { get; private set; }

        public void Initialize(CocApi cocApi)
        {
            if (BadgeUrl != null && ClanTag != null) BadgeUrl.ClanTag = ClanTag;
        }

        public override string ToString() => $"{ClanTag} {Name}";
    }
}
