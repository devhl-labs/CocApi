using devhl.CocApi.Models.Clan;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Cryptography;

namespace devhl.CocApi.Models.War
{
    public class WarClanBuilder
    {
        public string ClanTag { get; set; } = string.Empty;

        public string WarKey { get; internal set; } = string.Empty;

        //public IEnumerable<WarVillage>? WarVillages { get; internal set; }

        public int AttackCount { get; set; }

        public int DefenseCount { get; set; }

        public int Stars { get; set; }

        public decimal DestructionPercentage { get; set; }

        public Result Result { get; set; }

        public override string ToString() => $"{ClanTag}";

        public WarClan Build(string warKey)
        {
            return new WarClan
            {
                ClanTag = ClanTag,
                WarKey = warKey,
                AttackCount = AttackCount,
                DefenseCount = DefenseCount,
                Stars = Stars,
                DestructionPercentage = DestructionPercentage,
                Result = Result
            };
        }
    }
}
