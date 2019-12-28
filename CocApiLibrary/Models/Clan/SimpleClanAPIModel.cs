using System.ComponentModel.DataAnnotations;
////System.Text.Json.Serialization
using Newtonsoft.Json;

namespace devhl.CocApi.Models.Clan
{
    public class SimpleClanApiModel : IClanApiModel
    {
        // IClanApiModel

        //[JsonProperty("Tag")]
        [JsonProperty("tag")]
        public string ClanTag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public ClanBadgeUrlApiModel? BadgeUrls { get; set; }
    }
}
