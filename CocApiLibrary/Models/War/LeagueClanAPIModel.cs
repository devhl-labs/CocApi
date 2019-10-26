using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class LeagueClanAPIModel : IClanAPIModel
    {
        // IClanAPIModel
        [JsonPropertyName("Tag")]
        public string ClanTag { get; set; } = string.Empty;

        [NotMapped]
        public string Name { get; set; } = string.Empty;

        [NotMapped]
        public BadgeUrlModel? BadgeUrls { get; set; }

        [NotMapped]
        public int ClanLevel { get; set; }




        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// This is the season and the clan tag
        /// </summary>
        [Key]
        public string LeagueClanId { get; set; } = string.Empty;

        [ForeignKey(nameof(LeagueClanId))]
        public virtual IEnumerable<LeagueVillageAPIModel>? Villages { get; set; }
    }
}
