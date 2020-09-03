using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanWarAttack
    {
        [DataMember(Name = "attackerClanTag", EmitDefaultValue = false)]
        public string AttackerClanTag { get; internal set; }

        [DataMember(Name = "defenderClanTag", EmitDefaultValue = false)]
        public string DefenderClanTag { get; internal set; }

        [DataMember(Name = "starsGained", EmitDefaultValue = false)]
        public int StarsGained { get; internal set; }

        [DataMember(Name = "fresh", EmitDefaultValue = false)]
        public bool Fresh { get; internal set; } = false;

        [DataMember(Name = "attackerTownHall", EmitDefaultValue = false)]
        public int AttackerTownHall { get; internal set; }

        [DataMember(Name = "defenderTownHall", EmitDefaultValue = false)]
        public int DefenderTownHall { get; internal set; }

        [DataMember(Name = "attackerMapPosition", EmitDefaultValue = false)]
        public int AttackerMapPosition { get; internal set; }

        [DataMember(Name = "defenderMapPosition", EmitDefaultValue = false)]
        public int DefenderMapPosition { get; internal set; }
    }
}
