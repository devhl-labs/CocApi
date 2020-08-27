using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanWarAttack
    {
        public string AttackerClanTag { get; internal set; }

        public string DefenderClanTag { get; internal set; }

        public int StarsGained { get; internal set; }

        public bool Fresh { get; internal set; } = false;

        public int AttackerTownHall { get; internal set; }

        public int DefenderTownHall { get; internal set; }

        public int AttackerMapPosition { get; internal set; }

        public int DefenderMapPosition { get; internal set; }
    }
}
