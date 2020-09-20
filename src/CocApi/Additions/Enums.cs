using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CocApi
{
    ///// <summary>
    ///// Defines Role
    ///// </summary>
    //[JsonConverter(typeof(StringEnumConverter))]
    //public enum Role
    //{
    //    /// <summary>
    //    /// Enum Member for value: member
    //    /// </summary>
    //    [EnumMember(Value = "member")]
    //    Member = 1,

    //    /// <summary>
    //    /// Enum Admin for value: admin
    //    /// </summary>
    //    [EnumMember(Value = "admin")]
    //    Admin = 2,

    //    /// <summary>
    //    /// Enum CoLeader for value: coLeader
    //    /// </summary>
    //    [EnumMember(Value = "coLeader")]
    //    CoLeader = 3,

    //    /// <summary>
    //    /// Enum Leader for value: leader
    //    /// </summary>
    //    [EnumMember(Value = "leader")]
    //    Leader = 4

    //}

    [JsonConverter(typeof(StringEnumConverter))]
    public enum WarType
    {
        [EnumMember(Value = "unknown")]
        Unknown,
        [EnumMember(Value = "random")]
        Random,
        [EnumMember(Value = "friendly")]
        Friendly,
        [EnumMember(Value = "sccwl")]
        SCCWL
    }

    /// <summary>
    /// Defines Role
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Role
    {
        /// <summary>
        /// Enum Member for value: member
        /// </summary>
        [EnumMember(Value = "member")]
        Member = 0,

        /// <summary>
        /// Enum Admin for value: admin
        /// </summary>
        [EnumMember(Value = "admin")]
        Elder = 10,

        /// <summary>
        /// Enum CoLeader for value: coLeader
        /// </summary>
        [EnumMember(Value = "coLeader")]
        CoLeader = 20,

        /// <summary>
        /// Enum Leader for value: leader
        /// </summary>
        [EnumMember(Value = "leader")]
        Leader = 30

    }

    /// <summary>
    /// Defines Result
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Result
    {
        /// <summary>
        /// Enum Lose for value: lose
        /// </summary>
        [EnumMember(Value = "lose")]
        Lose = -1,

        /// <summary>
        /// Enum Tie for value: tie
        /// </summary>
        [EnumMember(Value = "tie")]
        Tie = 0,

        /// <summary>
        /// Enum Win for value: win
        /// </summary>
        [EnumMember(Value = "win")]
        Win = 1

    }

    /// <summary>
    /// Defines WarFrequency
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WarFrequency
    {
        /// <summary>
        /// Enum Unknown for value: unknown
        /// </summary>
        [EnumMember(Value = "unknown")]
        Unknown = 1,

        /// <summary>
        /// Enum Never for value: never
        /// </summary>
        [EnumMember(Value = "never")]
        Never = 2,

        /// <summary>
        /// Enum LessThanOncePerWeek for value: lessThanOncePerWeek
        /// </summary>
        [EnumMember(Value = "lessThanOncePerWeek")]
        LessThanOncePerWeek = 3,

        /// <summary>
        /// Enum OncePerWeek for value: oncePerWeek
        /// </summary>
        [EnumMember(Value = "oncePerWeek")]
        OncePerWeek = 4,

        /// <summary>
        /// Enum MoreThanOncePerWeek for value: moreThanOncePerWeek
        /// </summary>
        [EnumMember(Value = "moreThanOncePerWeek")]
        MoreThanOncePerWeek = 5,

        /// <summary>
        /// Enum Always for value: always
        /// </summary>
        [EnumMember(Value = "always")]
        Always = 6

    }

    /// <summary>
    /// Defines Type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RecruitingType
    {
        /// <summary>
        /// Enum InviteOnly for value: InviteOnly
        /// </summary>
        [EnumMember(Value = "InviteOnly")]
        InviteOnly = 1,

        /// <summary>
        /// Enum Closed for value: Closed
        /// </summary>
        [EnumMember(Value = "Closed")]
        Closed = 2,

        /// <summary>
        /// Enum Open for value: Open
        /// </summary>
        [EnumMember(Value = "Open")]
        Open = 3
    }

    /// <summary>
    /// Defines State
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GroupState
    {
        /// <summary>
        /// Enum Preparation for value: preparation
        /// </summary>
        [EnumMember(Value = "preparation")]
        Preparation = 1,

        /// <summary>
        /// Enum InWar for value: inWar
        /// </summary>
        [EnumMember(Value = "inWar")]
        InWar = 2,

        /// <summary>
        /// Enum Ended for value: ended
        /// </summary>
        [EnumMember(Value = "ended")]
        Ended = 3

    }

    /// <summary>
    /// Defines State
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WarState
    {
        /// <summary>
        /// Enum NotInWar for value: notInWar
        /// </summary>
        [EnumMember(Value = "notInWar")]
        NotInWar = 1,

        /// <summary>
        /// Enum Preparation for value: preparation
        /// </summary>
        [EnumMember(Value = "preparation")]
        Preparation = 2,

        /// <summary>
        /// Enum InWar for value: inWar
        /// </summary>
        [EnumMember(Value = "inWar")]
        InWar = 3,

        /// <summary>
        /// Enum WarEnded for value: warEnded
        /// </summary>
        [EnumMember(Value = "warEnded")]
        WarEnded = 4

    }
}
