using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
////System.Text.Json.Serialization
using Newtonsoft.Json;

using devhl.CocApi.Converters;
using devhl.CocApi.Models.Village;
using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.Clan
{
    public class ClanVillageApiModel : IVillageApiModel
    {
        // IVillageApiModel

        //[JsonProperty("Tag")]
        [JsonProperty("Tag")]
        public string VillageTag { get; set; } = string.Empty;


        public string Name { get; set; } = string.Empty;

        public string ClanTag { get; set; } = string.Empty;








        public Role Role { get; set; } = Role.Unknown;

        public int ExpLevel { get; set; }

        private VillageLeagueApiModel? _league;

        /// <summary>
        /// All leagues with the same Id will be the same instance
        /// </summary>
        public VillageLeagueApiModel? League
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
