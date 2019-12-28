using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
//System.Text.Json.Serialization

namespace devhl.CocApi.Models.War
{
    public class CurrentWarFlagsModel
    {

        public string WarId { get; set; } = string.Empty;


        public bool WarEndingSoon { get; set; } = false;


        public bool WarStartingSoon { get; set; } = false;


        public bool WarIsAccessible { get; set; } = true;


        public bool WarEndNotSeen { get; set; } = false;


        public bool WarAnnounced { get; set; } = false;


        public bool WarStarted { get; set; } = false;


        public bool WarEnded { get; set; } = false;


        public bool AttacksNotSeen { get; set; } = false;


        public bool AttacksMissed { get; set; } = false;


        public bool WarEndSeen { get; set; } = false;


        public CurrentWarApiModel? CurrentWarApiModel { get; set; }
    }
}
