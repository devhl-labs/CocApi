using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models.Clan
{
    public class CurrentWarFlags
    {
        [JsonIgnore]
        public bool WarEndingSoon { get; internal set; } = false;

        [JsonIgnore]
        public bool WarStartingSoon { get; internal set; } = false;

        [JsonIgnore]
        public bool WarIsAccessible { get; internal set; } = true;

        [JsonIgnore]
        public bool WarEndNotSeen { get; internal set; } = false;

        [JsonIgnore]
        public bool WarAnnounced { get; internal set; } = false;


        [JsonIgnore]
        public bool WarStarted { get; internal set; } = false;

        [JsonIgnore]
        public bool WarEnded { get; internal set; } = false;

        [JsonIgnore]
        public bool AttacksNotSeen { get; internal set; } = false;

        [JsonIgnore]
        public bool AttacksMissed { get; internal set; } = false;

        [JsonIgnore]
        public bool WarEndSeen { get; internal set; } = false;
    }
}
