using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            [EnumMember(Value = "unknown")]
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

        public enum WarState : int
        {
            [EnumMember(Value = "unknown")]
            Unknown = 0,
            [EnumMember(Value = "notInWar")]
            NotInWar = 10,
            [EnumMember(Value = "preparation")]
            Preparation = 20,
            [EnumMember(Value = "inWar")]
            InWar = 30,
            [EnumMember(Value = "warEnded")]
            WarEnded = 40,
            [EnumMember(Value = "ended")]
            LeagueWarsEnded = 50
            
        }

        public enum LeagueState : int
        {
            [EnumMember(Value = "unknown")]
            Unknown = 0,
            [EnumMember(Value = "notInWar")]
            NotInWar = 10,
            [EnumMember(Value = "preparation")]
            Preparation = 20,
            [EnumMember(Value = "inwar")]
            InWar = 30,
            [EnumMember(Value = "warEnded")]
            WarEnded = 40,
            [EnumMember(Value = "ended")]
            LeagueWarsEnded = 50

        }

        public enum Result : int
        {
            [EnumMember(Value = "undetermined")]
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

        public enum ClanType : int
        {
            InviteOnly = 0,
            Closed = 1,
            Open = 2
        }

        public enum Village : int
        {
            Home = 0,
            BuilderBase = 1
        }
    }

    //
    // Summary:
    //     Specifies the severity of the log message.
    public enum LogSeverity
    {
        //
        // Summary:
        //     Logs that contain the most severe level of error. This type of error indicate
        //     that immediate attention may be required.
        Critical = 0,
        //
        // Summary:
        //     Logs that highlight when the flow of execution is stopped due to a failure.
        Error = 1,
        //
        // Summary:
        //     Logs that highlight an abnormal activity in the flow of execution.
        Warning = 2,
        //
        // Summary:
        //     Logs that track the general flow of the application.
        Info = 3,
        //
        // Summary:
        //     Logs that are used for interactive investigation during development.
        Verbose = 4,
        //
        // Summary:
        //     Logs that contain the most detailed messages.
        Debug = 5
    }
}
