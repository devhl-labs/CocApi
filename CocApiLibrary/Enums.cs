using System.Runtime.Serialization;

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

        public enum RecruitmentType : int
        {
            [EnumMember(Value = "InviteOnly")]
            InviteOnly = 0,
            [EnumMember(Value = "Closed")]
            Closed = 1,
            [EnumMember(Value = "Open")]
            Open = 2
        }

        public enum VillageType : int
        {
            [EnumMember(Value = "Unknown")]
            Unknown = 0,
            [EnumMember(Value = "Home")]
            Home = 1,
            [EnumMember(Value = "BuilderBase")]
            BuilderBase = 2
        }
    }


    public enum EndPoint
    {
        Clan,
        Clans,
        Village,
        CurrentWar,
        LeagueGroup,
        LeagueWar,
        WarLog,
        VillageLeagues,
        Locations

    }

    public enum DownloadLeagueWars
    {
        /// <summary>
        /// False will never download league wars.
        /// </summary>
        False = 0,
        /// <summary>
        /// True will download league wars always.
        /// </summary>
        True = 1,
        /// <summary>
        /// Auto will download league wars only during the beginning of the month.
        /// </summary>
        Auto = 2
    }
}
