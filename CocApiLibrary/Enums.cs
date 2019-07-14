using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace CocApiLibrary
{
    public class Enums
    {
        public enum WarFrequency
        {
            [EnumMember(Value = "unknown")]
            unknown = 0,
            [EnumMember(Value = "always")]
            always = 1,
            [EnumMember(Value = "moreThanOncePerWeek")]
            moreThanOncePerWeek = 2,
            [EnumMember(Value = "oncePerWeek")]
            oncePerWeek = 3,
            [EnumMember(Value = "lessThanOncePerWeek")]
            lessThanOncePerWeek = 4,
            [EnumMember(Value = "never")]
            never = 5
        }

        public enum VerbosityType
        {
            None = 0,
            PreemptiveRateLimits = 1,
            Verbose = 2
        }

        public enum State : int
        {
            [EnumMember(Value = "notInWar")]
            notInWar = 0,
            [EnumMember(Value = "preparation")]
            preparation = 1,
            [EnumMember(Value = "inWar")]
            inWar = 2,
            [EnumMember(Value = "warEnded")]
            warEnded = 3
        }

    }
}
