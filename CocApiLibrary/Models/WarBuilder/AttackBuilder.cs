using Newtonsoft.Json;
using System;


namespace devhl.CocApi.Models.War
{
    public class AttackBuilder
    {
        internal DateTime PreparationStartTimeUtc { get; private set; }

        public DateTime DownloadedUtc { get; set; } = DateTime.UtcNow;

        internal string WarKey { get; private set; } = string.Empty;

        public string AttackerTag { get; set; } = string.Empty;


#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        public string? DefenderTag { get; set; }

#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.


        public int? Stars { get; set; }

        public int? StarsGained { get; set; }

        public int? DestructionPercentage { get; set; }

        public int? Order { get; set; }

        public bool Fresh { get; set; }

        public string AttackerClanTag { get; set; } = string.Empty;

        public string DefenderClanTag { get; set; } = string.Empty;

        public int AttackerMapPosition { get; set; }

        public int? DefenderMapPosition { get; set; }

        public int AttackerTownHallLevel { get; set; }

        public int? DefenderTownHallLevel { get; set; }

        public override string ToString() => $"{AttackerClanTag} {AttackerTag}";

        internal Attack Build(string warKey, DateTime preparationStartTime)
        {
            return new Attack
            {
                WarKey = warKey,
                PreparationStartTimeUtc = preparationStartTime,
                DownloadedUtc = DownloadedUtc,
                AttackerTag = AttackerTag,
                DefenderTag = DefenderTag,
                Stars = Stars,
                StarsGained = StarsGained,
                DestructionPercentage = DestructionPercentage,
                Order = Order,
                Fresh = Fresh,
                AttackerClanTag = AttackerClanTag,
                DefenderClanTag = DefenderClanTag,
                AttackerMapPosition = AttackerMapPosition,
                DefenderMapPosition = DefenderMapPosition,
                DefenderTownHallLevel = DefenderTownHallLevel,
                AttackerTownHallLevel = AttackerTownHallLevel
            };
        }
    }
}
