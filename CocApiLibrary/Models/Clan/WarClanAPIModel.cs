using System.Collections.Generic;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class WarClanAPIModel : IClanAPIModel
    {
        public string Tag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public BadgeUrlModel? BadgeUrls { get; set; }

        public int ClanLevel { get; set; }

        public IEnumerable<MemberAPIModel>? Members { get; set; }





        [JsonPropertyName("attacks")]
        public int AttackCount { get; set; }

        public int DefenseCount { get; set; }

        public int Stars { get; set; }

        public decimal DestructionPercentage { get; set; }







        [JsonIgnore]
        public IList<AttackAPIModel> Attacks { get; set; } = new List<AttackAPIModel>();

        [JsonIgnore]
        public IList<AttackAPIModel> Defenses { get; set; } = new List<AttackAPIModel>();

        [JsonIgnore]
        public Result Result { get; set; }

        //[JsonIgnore]
        //public bool? WarIsAccessible { get; internal set; } = null;

    }
}
