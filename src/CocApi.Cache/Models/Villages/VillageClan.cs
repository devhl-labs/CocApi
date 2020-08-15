using CocApi.Cache.Models.Clans;

using Newtonsoft.Json;

namespace CocApi.Cache.Models.Villages
{
    public class VillageClan : IClan
    {
        [JsonProperty("Tag")]
        public string ClanTag { get; private set; } = string.Empty;

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;

        [JsonProperty]
        public BadgeUrl? BadgeUrl { get; private set; }

        [JsonProperty]
        public int ClanLevel { get; private set; }

        public override string ToString() => $"{ClanTag} {Name}";
    }
}
