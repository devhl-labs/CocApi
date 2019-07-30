using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class LeagueClanAPIModel : IClanAPIModel
    {
        public string Tag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public BadgeUrlModel? BadgeUrls { get; set; }

        public int ClanLevel { get; set; }

        public IEnumerable<LeagueMemberAPIModel>? Members { get; set; }
    }
}
