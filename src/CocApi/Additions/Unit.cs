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

        public string? SuperTroopName { get; }

        public Unit(string name, int maxLevel, Clash.Village village, Clash.Resource resource, string? superTroopName = null)
        {
            Name = name;
            MaxLevel = maxLevel;
            Village = village;
            Resource = resource;
            SuperTroopName = superTroopName;
        }
    }
}
