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
}
