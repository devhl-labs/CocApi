using CocApiLibrary.Converters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class ClanVillageAPIModel : IVillageAPIModel
    {
        // IVillageAPIModel
        [Key]
        [JsonPropertyName("Tag")]
        public string VillageTag { get; set; } = string.Empty;

        [NotMapped]
        public string Name { get; set; } = string.Empty;

        public string ClanTag { get; set; } = string.Empty;







        [JsonConverter(typeof(RoleConverter))]
        public Role Role { get; set; } = Role.Unknown;

        public int ExpLevel { get; set; }

        private VillageLeagueAPIModel? _league;

        /// <summary>
        /// All leagues with the same Id will be the same instance
        /// </summary>
        [ForeignKey(nameof(LeagueId))]
        public virtual VillageLeagueAPIModel? League
        {
            get
            {
                return _league;
            }
        
            set
            {
                _league = value;

                if (_league != null)
                {
                    LeagueId = _league.Id;
                }
            }
        }

        public int? LeagueId { get; set; }

        public int Trophies { get; set; }

        public int VersusTrophies { get; set; }

        public int ClanRank { get; set; }

        public int PreviousClanRank { get; set; }

        public int Donations { get; set; }

        public int DonationsReceived { get; set; }
    }
}
