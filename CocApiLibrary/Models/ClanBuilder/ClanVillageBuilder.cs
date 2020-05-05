using Newtonsoft.Json;
using devhl.CocApi.Models.Village;

namespace devhl.CocApi.Models.Clan
{
    public class ClanVillageBuilder
    {
        public string VillageTag { get; set; } = string.Empty;


        public string Name { get; set; } = string.Empty;


        public string ClanTag { get; set; } = string.Empty;


        public Role Role { get; set; } = Role.Unknown;


        public int ExpLevel { get; set; }


        //public League? League { get; set; }


        public int? LeagueId { get; set; }


        public int Trophies { get; set; }


        public int VersusTrophies { get; set; }


        public int ClanRank { get; set; }


        public int PreviousClanRank { get; set; }


        public int Donations { get; set; }


        public int DonationsReceived { get; set; }

        public override string ToString() => $"{VillageTag} {Name} {Role}";

        public ClanVillage Build()
        {
            ClanVillage clanVillage = new ClanVillage
            {
                VillageTag = VillageTag,
                Name = Name,
                ClanTag = ClanTag,
                Role = Role,
                ExpLevel = ExpLevel,
                LeagueId = LeagueId,
                Trophies = Trophies,
                VersusTrophies = VersusTrophies,
                ClanRank = ClanRank,
                PreviousClanRank = PreviousClanRank,
                Donations = Donations,
                DonationsReceived = DonationsReceived
            };

            return clanVillage;
        }
    }
}
