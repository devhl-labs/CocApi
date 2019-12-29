using devhl.CocApi.Models.Clan;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class VillageClan : IClan
    {
        [JsonProperty("Tag")]
        public string ClanTag { get; } = string.Empty;

        [JsonProperty]
        public string Name { get; } = string.Empty;

        [JsonProperty]
        public BadgeUrl? BadgeUrl { get; }

        [JsonProperty]
        public int ClanLevel { get; }

        public void Initialize()
        {
        }
    }
}
