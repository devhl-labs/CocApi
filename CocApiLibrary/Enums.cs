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
            WarEnded = 40            
        }

        public enum LeagueState : int
        {
            [EnumMember(Value = "unknown")]
            Unknown = 0,
            [EnumMember(Value = "notInWar")]
            NotInWar = 10,
            [EnumMember(Value = "preparation")]
            Preparation = 20,
            [EnumMember(Value = "inWar")]
            InWar = 30,
            [EnumMember(Value = "ended")]
            WarsEnded = 50

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
            [EnumMember(Value = "Random")]
            Random = 0,
            [EnumMember(Value = "Friendly")]
            Friendly = 1,
            [EnumMember(Value = "SCCWL")]
            SCCWL = 2
        }

        public enum ClanType : int
        {
            [EnumMember(Value = "InviteOnly")]
            InviteOnly = 0,
            [EnumMember(Value = "Closed")]
            Closed = 1,
            [EnumMember(Value = "Open")]
            Open = 2
        }

        public enum Village : int
        {
            [EnumMember(Value = "Home")]
            Home = 0,
            [EnumMember(Value = "BuilderBase")]
            BuilderBase = 1
        }
    }
    
    public enum LogSeverity
    {
        /// <summary>
        /// This type of error indicate that immediate attention may be required.
        /// </summary>
        Critical = 0,
        /// <summary>
        /// Logs that highlight when the flow of execution is stopped due to a failure.
        /// </summary>   
        Error = 1,
        /// <summary>
        /// Logs that highlight an abnormal activity in the flow of execution.
        /// </summary>
        Warning = 2,
        /// <summary>
        /// Logs that track the general flow of the application.
        /// </summary>
        Info = 3,
        /// <summary>
        /// Logs that are used for interactive investigation during development.
        /// </summary>
        Verbose = 4,   
        /// <summary>
        /// Logs that contain the most detailed messages.
        /// </summary>
        Debug = 5
    }

    public enum EndPoint
    {
        Clan,
        Clans,
        Village,
        CurrentWar,
        LeagueGroup,
        LeagueWar,
        WarLog

    }
}
