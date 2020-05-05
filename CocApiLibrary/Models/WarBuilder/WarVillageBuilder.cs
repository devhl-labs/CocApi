using devhl.CocApi.Models.Village;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class WarVillageBuilder
    {
        public string VillageTag { get; set; } = string.Empty;

        public string ClanTag { get; set; } = string.Empty;

        internal IList<Attack>? Attacks { get; set; }

        internal string WarKey { get; set; } = string.Empty;

        public override string ToString() => $"{VillageTag}";

        public string Name { get; set; } = string.Empty;

        public int TownHallLevel { get; set; }

        public int RosterPosition { get; set; }

        public int MapPosition { get; set; }

        public WarVillage Build(string warKey)
        {
            return new WarVillage
            {
                WarKey = warKey,
                VillageTag = VillageTag,
                ClanTag = ClanTag,
                Name = Name,
                TownHallLevel = TownHallLevel,
                RosterPosition = RosterPosition,
                MapPosition = MapPosition
            };
        }
    }
}
