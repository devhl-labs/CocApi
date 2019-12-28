using devhl.CocApi.Models.Clan;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
////System.Text.Json.Serialization
using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class LeagueClanApiModel : IClanApiModel
    {
        // IClanApiModel
        [JsonProperty("Tag")]
        public string ClanTag { get; set; } = string.Empty;

        [NotMapped]
        public string Name { get; set; } = string.Empty;

        [NotMapped]
        public ClanBadgeUrlApiModel? BadgeUrls { get; set; }








        [NotMapped]
        public int ClanLevel { get; set; }

        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// This is the season and the clan tag
        /// </summary>
        [Key]
        public string LeagueClanId { get; set; } = string.Empty;

        [ForeignKey(nameof(LeagueClanId))]
        [JsonProperty("members")]
        public virtual IEnumerable<LeagueVillageApiModel>? Villages { get; set; }
    }
}
