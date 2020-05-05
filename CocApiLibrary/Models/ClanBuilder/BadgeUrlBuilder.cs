using Newtonsoft.Json;
using System.Linq;

namespace devhl.CocApi.Models.Clan
{
    public class BadgeUrlBuilder
    {
        public string ClanTag { get; set; } = string.Empty;

        public string Small { get; set; } = string.Empty;
        
        public string Large { get; set; } = string.Empty;

        public string Medium { get; set; } = string.Empty;

        internal BadgeUrl Build()
        {
            BadgeUrl badgeUrl = new BadgeUrl
            {
                ClanTag = ClanTag,
                Small = Small,
                Large = Large,
                Medium = Medium
            };

            return badgeUrl;
        }
    }
}
