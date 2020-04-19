using Newtonsoft.Json;
using System;


namespace devhl.CocApi.Models.War
{
    public class Attack : /*IInitialize,*/ IAttack
    {
        [JsonProperty]
        public DateTime PreparationStartTimeUtc { get; internal set; }

        public DateTime DownloadedUtc { get; internal set; } = DateTime.UtcNow;

        [JsonProperty]
        public string WarKey { get; internal set; } = string.Empty;

        [JsonProperty]
        public string AttackerTag { get; internal set; } = string.Empty;

        [JsonProperty]
#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        public string? DefenderTag { get; internal set; }

#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        [JsonProperty]
        public int? Stars { get; private set; }

        [JsonProperty]
        public int? StarsGained { get; internal set; }

        [JsonProperty]
        public int? DestructionPercentage { get; private set; }

        [JsonProperty]
        public int? Order { get; private set; }

        [JsonProperty]
        public bool? Fresh { get; internal set; } = false;

        [JsonProperty]
        public string AttackerClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string DefenderClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public int AttackerMapPosition { get; internal set; }

        [JsonProperty]
        public int? DefenderMapPosition { get; internal set; }

        [JsonProperty]
        public int AttackerTownHallLevel { get; internal set; }

        [JsonProperty]
        public int? DefenderTownHallLevel { get; internal set; }

        [JsonProperty]
        public bool Missed { get; internal set; }

        public override string ToString() => $"{AttackerClanTag} {AttackerTag}";
    }
}
