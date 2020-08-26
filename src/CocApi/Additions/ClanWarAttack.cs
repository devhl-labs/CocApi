using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanWarAttack
    {
        public string AttackerClanTag { get; set; }

        public string DefenderClanTag { get; set; }

        public int StarsGained { get; set; }

        public bool Fresh { get; set; } = false;

        public int AttackerTownHall { get; set; }

        public int DefenderTownHall { get; set; }

        public int AttackerMapPosition { get; set; }

        public int DefenderMapPosition { get; set; }
    }
}
