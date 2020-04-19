using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.Text;


namespace devhl.CocApi.Models.War
{
    public class CurrentWarFlags
    {

        public string WarKey { get; internal set; } = string.Empty;


        public bool WarEndingSoon { get; internal set; } = false;


        public bool WarStartingSoon { get; internal set; } = false;


        public bool WarIsAccessible { get; internal set; } = true;


        public bool WarEndNotSeen { get; internal set; } = false;


        public bool WarAnnounced { get; internal set; } = false;


        public bool WarStarted { get; internal set; } = false;


        public bool WarEnded { get; internal set; } = false;


        public bool AttacksNotSeen { get; internal set; } = false;


        public bool AttacksMissed { get; internal set; } = false;


        public bool WarEndSeen { get; internal set; } = false;
    }
}
