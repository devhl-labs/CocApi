using devhl.CocApi.Models.Clan;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class LeagueClan : IClan
    {
        [JsonProperty("Tag")]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;


        [JsonProperty]
        public BadgeUrl? BadgeUrl { get; internal set; }


        [JsonProperty]
        public int ClanLevel { get; private set; }

        [JsonProperty("members")]
        public IEnumerable<LeagueVillage>? Villages { get; internal set; }

        public override string ToString() => $"{ClanTag} {Name}";
    }
}
