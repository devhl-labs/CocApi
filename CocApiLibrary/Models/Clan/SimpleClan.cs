using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.Clan
{
    public class SimpleClan : IClan
    {
        [JsonProperty("tag")]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty]
        public ClanBadgeUrl? BadgeUrls { get; internal set; }

        public void Initialize()
        {
            if (BadgeUrls != null) BadgeUrls.Initialize();
        }
    }
}
