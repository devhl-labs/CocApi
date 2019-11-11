using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models
{
    public class SimpleClanApiModel : IClanApiModel
    {
        // IClanApiModel
        [Key]
        [JsonPropertyName("Tag")]
        public string ClanTag { get; set; } = string.Empty;

        public int ClanLevel { get; set; }

        public string Name { get; set; } = string.Empty;

        public ClanBadgeUrlApiModel? BadgeUrls { get; set; }
    }
}
