using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class CurrentWarFlagsModel
    {
        [Key]
        public string WarId { get; set; } = string.Empty;

        [JsonIgnore]
        public bool WarEndingSoon { get; set; } = false;

        [JsonIgnore]
        public bool WarStartingSoon { get; set; } = false;

        [JsonIgnore]
        public bool WarIsAccessible { get; set; } = true;

        [JsonIgnore]
        public bool WarEndNotSeen { get; set; } = false;

        [JsonIgnore]
        public bool WarAnnounced { get; set; } = false;


        [JsonIgnore]
        public bool WarStarted { get; set; } = false;

        [JsonIgnore]
        public bool WarEnded { get; set; } = false;

        [JsonIgnore]
        public bool AttacksNotSeen { get; set; } = false;

        [JsonIgnore]
        public bool AttacksMissed { get; set; } = false;

        [JsonIgnore]
        public bool WarEndSeen { get; set; } = false;

        [JsonIgnore]
        public virtual CurrentWarApiModel? CurrentWarApiModel { get; set; }
    }
}
