using CocApi.Cache.Models.Clans;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using CocApi.Cache.Converters;

namespace CocApi.Cache.Models.Wars
{
    public class WarLogEntryClan : IClan, IWarClan
    {
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

        [JsonProperty("tag")]
        public string? ClanTag { get; internal set; }

        [JsonProperty]
        public string? Name { get; private set; }

#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).


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

        public override string ToString() => $"{ClanTag} {Name}";
    }
}
