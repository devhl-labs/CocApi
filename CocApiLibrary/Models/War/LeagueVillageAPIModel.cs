using devhl.CocApi.Models.Village;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.War
{
    public class LeagueVillageApiModel : IVillageApiModel
    {
        // IVillageApiModel
        [Key]
        [JsonPropertyName("Tag")]
        public string VillageTag { get; set; } = string.Empty;

        [NotMapped]
        public string Name { get; set; } = string.Empty;

        [NotMapped]
        public string ClanTag { get; set; } = string.Empty;








        [NotMapped]
        public int TownhallLevel { get; set; }

        public string LeagueClanId { get; set; } = string.Empty;
    }
}
