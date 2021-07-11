using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocApi
{
    public class Unit
    {
        public string Name { get; }

        public int DisplayOrder { get; }

        public int MaxLevel { get; }

        public Clash.Village Village { get; }

        public Clash.Resource Resource { get; }

        public int? TroopId { get; }

        public bool IsSeasonalTroop { get; }

        public Unit? BaseTroop { get; }

        internal Unit(Clash.Village village, Clash.Resource resource, int displayOrder, int maxLevel, string name, int? troopId = null, bool isSeasonalTroop = false)
        {
            Name = name;
            DisplayOrder = displayOrder;
            MaxLevel = maxLevel;
            Village = village;
            Resource = resource;
            TroopId = troopId;
            IsSeasonalTroop = isSeasonalTroop;
        }

        internal Unit(Clash.Village village, Clash.Resource resource, int displayOrder, Unit baseTroop, string name, int? troopId = null)
            : this(village, resource, displayOrder, baseTroop.MaxLevel, name, troopId)
        {
            BaseTroop = baseTroop;
        }
    }
}
