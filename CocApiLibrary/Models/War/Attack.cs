using Newtonsoft.Json;
using System;


namespace devhl.CocApi.Models.War
{
    public class Attack
    {
        [JsonProperty]
        public DateTime PreparationStartTimeUtc { get; internal set; }

        [JsonProperty]
        public string WarId { get; internal set; } = string.Empty;

        [JsonProperty]
        public string AttackerTag { get; } = string.Empty;

        [JsonProperty]
        public string DefenderTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public int Stars { get; }

        [JsonProperty]
        public int StarsGained { get; internal set; }

        [JsonProperty]
        public int DestructionPercentage { get; }

        [JsonProperty]
        public int Order { get; }

        [JsonProperty]
        public bool Fresh { get; internal set; } = false;

        [JsonProperty]
        public string AttackerClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string DefenderClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public int AttackerMapPosition { get; internal set; }

        [JsonProperty]
        public int DefenderMapPosition { get; internal set; }

        [JsonProperty]
        public int AttackerTownHallLevel { get; internal set; }

        [JsonProperty]
        public int DefenderTownHallLevel { get; internal set; }
    }
}
