using devhl.CocApi.Models.Clan;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.Village
{
    public class VillageClanApiModel : IClanApiModel
    {
        // IClanApiModel
        [Key]
        [JsonPropertyName("Tag")]
        public string ClanTag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public ClanBadgeUrlApiModel? BadgeUrls { get; set; }




        public int ClanLevel { get; set; }
    }
}
