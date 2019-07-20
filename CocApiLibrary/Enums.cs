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
            unknown = 0,
            [EnumMember(Value = "never")]
            never = 10,
            [EnumMember(Value = "lessThanOncePerWeek")]
            lessThanOncePerWeek = 20,
            [EnumMember(Value = "oncePerWeek")]
            oncePerWeek = 30,
            [EnumMember(Value = "moreThanOncePerWeek")]
            moreThanOncePerWeek = 40,
            [EnumMember(Value = "always")]
            always = 50
        }

        public enum Role : int
        {
            [EnumMember(Value = "member")]
            member = 0,
            [EnumMember(Value = "admin")]
            elder = 10,
            [EnumMember(Value = "coLeader")]
            coleader = 20,
            [EnumMember(Value = "leader")]
            leader = 30
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

        public enum Result : int
        {
            undetermined = 0,
            win = 1,
            lose = 2,
            draw = 3

        }



    }
}
