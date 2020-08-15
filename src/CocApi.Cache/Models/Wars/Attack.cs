using Newtonsoft.Json;
using System;


namespace CocApi.Cache.Models.Wars
{
    public class Attack : IAttack
    {
        public DateTime DownloadedUtc { get; internal set; } = DateTime.UtcNow;

        [JsonProperty]
        public string AttackerTag { get; internal set; } = string.Empty;



#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

        [JsonProperty]
        public string? DefenderTag { get; internal set; }


#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).


        [JsonProperty]
        public int? Stars { get; internal set; }

        [JsonProperty]
        public int? StarsGained { get; internal set; }

        [JsonProperty]
        public int? DestructionPercentage { get; internal set; }

        [JsonProperty]
        public int? Order { get; internal set; }

        [JsonProperty]
        public bool Fresh { get; internal set; }

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

        public override string ToString() => $"{AttackerClanTag} {AttackerTag}";
    }
}
