using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class MemberAPIModel : IVillageAPIModel
    {
        public string ClanTag { get; set; } = string.Empty;

        public string Tag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int TownhallLevel { get; set; }

        public int MapPosition { get; set; }

        public IList<AttackAPIModel>? Attacks { get; set; }
    }
}
