using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class MemberAPIModel : IVillageAPIModel
    {
        public string Tag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int TownhallLevel { get; set; }

        public int MapPosition { get; set; }

        //[JsonPropertyName("OpponentAttacks")]
        //public int DefenseCount { get; set; }

        //public AttackAPIModel? BestOpponentAttack { get; set; }

        public IList<AttackAPIModel>? Attacks { get; set; }





        //[JsonIgnore]
        //public IList<AttackAPIModel> Defenses { get; set; } = new List<AttackAPIModel>();
    }
}
