//using System;
//using System.Collections.Generic;
//using System.Text;


//namespace devhl.CocApi.Models.War
//{
//    public class CurrentWarFlagsBuilder
//    {

//        public string WarKey { get; internal set; } = string.Empty;

//        public bool WarEndingSoon { get; set; } = false;

//        public bool WarStartingSoon { get; set; } = false;

//        public bool WarIsAccessible { get; set; } = true;

//        public bool WarEndNotSeen { get; set; } = false;

//        public bool WarAnnounced { get; set; } = false;

//        public bool WarStarted { get; set; } = false;

//        public bool WarEnded { get; set; } = false;

//        public bool AttacksNotSeen { get; set; } = false;

//        public bool AttacksMissed { get; set; } = false;

//        public bool WarEndSeen { get; set; } = false;

//        internal CurrentWarFlags Build(string warKey)
//        {
//            return new CurrentWarFlags
//            {
//                AttacksMissed = AttacksMissed,
//                AttacksNotSeen = AttacksNotSeen,
//                WarAnnounced = WarAnnounced,
//                WarEnded = WarEnded,
//                WarEndingSoon = WarEndingSoon,
//                WarEndNotSeen = WarEndNotSeen,
//                WarEndSeen = WarEndSeen,
//                WarIsAccessible = WarIsAccessible,
//                WarKey = warKey,
//                WarStarted = WarStarted,
//                WarStartingSoon = WarStartingSoon,
//            };
//        }
//    }
//}
