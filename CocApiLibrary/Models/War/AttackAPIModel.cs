using System;
//System.Text.Json.Serialization

namespace devhl.CocApi.Models.War
{
    public class AttackApiModel
    {
        public DateTime PreparationStartTimeUtc { get; set; }

        public string WarId { get; set; } = string.Empty;

        public string AttackerTag { get; set; } = string.Empty;

        public string DefenderTag { get; set; } = string.Empty;

        public int Stars { get; set; }

        public int StarsGained { get; set; }

        public int DestructionPercentage { get; set; }

        public int Order { get; set; }




        public bool Fresh { get; set; } = false;

        public string AttackerClanTag { get; set; } = string.Empty;

        public string DefenderClanTag { get; set; } = string.Empty;

        public int AttackerMapPosition { get; set; }

        public int DefenderMapPosition { get; set; }

        public int AttackerTownHallLevel { get; set; }

        public int DefenderTownHallLevel { get; set; }
    }
}
