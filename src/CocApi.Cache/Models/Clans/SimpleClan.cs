using Newtonsoft.Json;

namespace CocApi.Cache.Models.Clans
{
    public class SimpleClan : IClan
    {
        [JsonProperty("tag")]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty]
        public BadgeUrl? BadgeUrl { get; internal set; }

        public override string ToString()
        {
            return $"{ClanTag} {Name}";
        }
    }
}
