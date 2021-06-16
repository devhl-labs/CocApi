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

        public int MaxLevel { get; }

        public Clash.Village Village { get; }

        public Clash.Resource Resource { get; }

        public int? TroopId { get; }

        public Unit? BaseTroop { get; }

        internal Unit(Clash.Village village, Clash.Resource resource, int maxLevel, string name, int? troopId = null, Unit? baseTroop = null)
        {
            Name = name;
            BaseTroop = baseTroop;
            MaxLevel = maxLevel;
            Village = village;
            Resource = resource;
            TroopId = troopId;
        }
    }
}
