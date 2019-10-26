using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class AttackAPIModel
    {
        public DateTime PreparationStartTimeUtc { get; set; }

        public string WarId { get; set; } = string.Empty;

        public string AttackerTag { get; set; } = string.Empty;

        public string DefenderTag { get; set; } = string.Empty;

        public int Stars { get; set; }

        public int DestructionPercentage { get; set; }

        public int Order { get; set; }




        [JsonIgnore]
        public bool Fresh { get; set; } = false;

        [JsonIgnore]
        public string AttackerClanTag { get; set; } = string.Empty;

        [JsonIgnore]
        public string DefenderClanTag { get; set; } = string.Empty;
    }
}
