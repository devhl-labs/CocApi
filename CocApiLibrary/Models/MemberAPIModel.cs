using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CocApiStandardLibrary.Models
{
    public class MemberAPIModel : IVillageAPIModel
    {
        public string Tag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int TownhallLevel { get; set; }

        public int MapPosition { get; set; }

        public int OpponentAttacks { get; set; }

        public BestOpponentAttackAPIModel? BestOpponentAttack { get; set; }

        public IEnumerable<AttackAPIModel>? Attacks { get; set; }

        public string ClanTag { get; set; } = string.Empty;





        [JsonIgnore]
        public IList<AttackAPIModel> Defenses { get; set; } = new List<AttackAPIModel>();
    }
}
