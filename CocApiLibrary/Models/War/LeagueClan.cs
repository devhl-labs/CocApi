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
        public string Name { get; } = string.Empty;


        [JsonProperty]
        public ClanBadgeUrl? BadgeUrls { get; }


        [JsonProperty]
        public int ClanLevel { get; }

        [JsonProperty]
        public string GroupId { get; internal set; } = string.Empty;

        /// <summary>
        /// This is the season and the clan tag
        /// </summary>
        [JsonProperty]
        public string LeagueClanId { get; internal set; } = string.Empty;

        [JsonProperty("members")]
        public IEnumerable<LeagueVillage>? Villages { get; }

        public void Initialize()
        {
            if (BadgeUrls != null) BadgeUrls.Initialize();
        }
    }
}
