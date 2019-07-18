using CocApiLibrary.Converters;
using System.Text.Json.Serialization;
using static CocApiStandardLibrary.Enums;

namespace CocApiStandardLibrary.Models
{
    public class MemberListAPIModel : IVillageAPIModel
    {
        public string Tag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        [JsonConverter(typeof(RoleConverter))]
        public Role Role { get; set; }

        public int ExpLevel { get; set; }

        public LeagueAPIModel? League { get; set; }

        public int Trophies { get; set; }

        public int VersusTrophies { get; set; }

        public int ClanRank { get; set; }

        public int PreviousClanRank { get; set; }

        public int Donations { get; set; }

        public int DonationsReceived { get; set; }
        
        public string ClanTag { get; set; } = string.Empty;
    }
}
