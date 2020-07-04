using Newtonsoft.Json;
using devhl.CocApi.Models.Village;

namespace devhl.CocApi.Models.Clan
{
    public class ClanVillage : IVillage
    {
        [JsonProperty("tag")]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty]
        public Role Role { get; internal set; } = Role.Unknown;

        [JsonProperty]
        public int ExpLevel { get; internal set; }

        [JsonProperty]
        public League? League { get; internal set; }

        [JsonProperty]
        public int Trophies { get; internal set; }

        [JsonProperty]
        public int VersusTrophies { get; internal set; }

        [JsonProperty]
        public int ClanRank { get; internal set; }

        [JsonProperty]
        public int PreviousClanRank { get; internal set; }

        [JsonProperty]
        public int Donations { get; internal set; }

        [JsonProperty]
        public int DonationsReceived { get; internal set; }

        public override string ToString() => $"{VillageTag} {Name} {Role}";
    }
}
