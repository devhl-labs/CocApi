using System;
using System.Runtime.Serialization;

namespace devhl.CocApi
{
    public enum CacheOption
    {
        AllowAny,
        AllowExpiredServerResponse,
        //AllowLocallyExpired,
        FetchIfExpired
    }

    public enum WarFrequency
    {
        [EnumMember(Value = "unknown")]
        Unknown,
        [EnumMember(Value = "never")]
        Never,
        [EnumMember(Value = "lessThanOncePerWeek")]
        LessThanOncePerWeek,
        [EnumMember(Value = "oncePerWeek")]
        OncePerWeek,
        [EnumMember(Value = "moreThanOncePerWeek")]
        MoreThanOncePerWeek,
        [EnumMember(Value = "always")]
        Always
    }

    public enum Role
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

    public enum WarState
    {
        [EnumMember(Value = "unknown")]
        Unknown,
        [EnumMember(Value = "notInWar")]
        NotInWar,
        [EnumMember(Value = "preparation")]
        Preparation,
        [EnumMember(Value = "inWar")]
        InWar,
        [EnumMember(Value = "warEnded")]
        WarEnded,
        //[EnumMember(Value = "warEndNotSeen")]
        //WarEndNotSeen
    }

    public enum LeagueState
    {
        [EnumMember(Value = "unknown")]
        Unknown,
        [EnumMember(Value = "notInWar")]
        NotInWar,
        [EnumMember(Value = "preparation")]
        Preparation,
        [EnumMember(Value = "inWar")]
        InWar,
        [EnumMember(Value = "ended")]
        WarsEnded

    }

    public enum Result
    {
        [EnumMember(Value = "lose")]
        Lose = -1,
        [EnumMember(Value = null)]
        Null = 0,
        [EnumMember(Value = "win")]
        Win = 1,
        [EnumMember(Value = "tie")]
        Tie = 2
    }

    public enum WarType
    {
        [EnumMember(Value = "Random")]
        Random,
        [EnumMember(Value = "Friendly")]
        Friendly,
        [EnumMember(Value = "SCCWL")]
        SCCWL
    }

    public enum RecruitmentType
    {
        [EnumMember(Value = "unknown")]
        Unknown,
        [EnumMember(Value = "InviteOnly")]
        InviteOnly,
        [EnumMember(Value = "Closed")]
        Closed,
        [EnumMember(Value = "Open")]
        Open
    }

    public enum VillageType
    {
        [EnumMember(Value = "unknown")]
        Unknown,
        [EnumMember(Value = "Home")]
        Home,
        [EnumMember(Value = "BuilderBase")]
        BuilderBase
    }

    public enum EndPoint
    {
        Clan,
        Clans,
        Village,
        CurrentWar,
        LeagueGroup,
        LeagueWar,
        WarLogEntries,
        VillageLeagues,
        Locations,
        Labels,
        WarLeagues,
        TopBuilderVillages,
        TopMainVillages,
        TopBuilderClans,
        TopMainClans
    }

    //public enum DownloadCwl
    //{
    //    /// <summary>
    //    /// False will never download league wars.
    //    /// </summary>
    //    False = 0,
    //    /// <summary>
    //    /// True will download league wars always.
    //    /// </summary>
    //    True = 1,
    //    /// <summary>
    //    /// Auto will download league wars only during the beginning of the month.
    //    /// </summary>
    //    Auto = 2
    //}

    public enum LoggingEvent
    {
        Unknown,
        QueueExited,
        QueueRestartFailed,
        Exception,
        RateLimited
    }

    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
        None
    }

    [Flags]
    public enum Announcements
    {
        None = 0,
        WarEndingSoon = 1,
        WarStartingSoon = 2,
        WarIsAccessible = 4,
        WarEndNotSeen = 8,
        WarAnnounced = 16,
        WarStarted = 32,
        WarEnded = 64,
        NotUsed = 128,
        AttacksMissed = 256,
        WarEndSeen = 512,
        AddedToWarLog = 1024
    }
}
