using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace CocApiLibrary
{
    public class Enums
    {
        public enum WarFrequency : int
        {
            [EnumMember(Value = "unknown")]
            Unknown = 0,
            [EnumMember(Value = "never")]
            Never = 10,
            [EnumMember(Value = "lessThanOncePerWeek")]
            LessThanOncePerWeek = 20,
            [EnumMember(Value = "oncePerWeek")]
            OncePerWeek = 30,
            [EnumMember(Value = "moreThanOncePerWeek")]
            MoreThanOncePerWeek = 40,
            [EnumMember(Value = "always")]
            Always = 50
        }

        public enum Role : int
        {
            Unknown = 0,
            [EnumMember(Value = "member")]
            Member = 10,
            [EnumMember(Value = "admin")]
            Elder = 20,
            [EnumMember(Value = "coLeader")]
            Coleader = 30,
            [EnumMember(Value = "leader")]
            Leader = 40
        }

        public enum VerbosityType : int
        {
            None = 0,
            PreemptiveRateLimits = 1,
            Verbose = 2
        }

        public enum State : int
        {
            [EnumMember(Value = "notInWar")]
            NotInWar = 0,
            [EnumMember(Value = "preparation")]
            Preparation = 10,
            [EnumMember(Value = "inWar")]
            InWar = 20,
            [EnumMember(Value = "warEnded")]
            WarEnded = 30
        }

        public enum LeagueState : int
        {
            [EnumMember(Value ="ended")]
            Ended = 1 //todo add the other states for league groups
        }

        public enum Result : int
        {
            Undetermined = 0,
            [EnumMember(Value = "win")]
            Win = 1,
            [EnumMember(Value = "lose")]
            Lose = 2,
            [EnumMember(Value = "draw")]
            Draw = 3
        }

        public enum WarType : int
        {
            Random = 0,
            Friendly = 1,
            SCCWL = 2
        }


    }
}
