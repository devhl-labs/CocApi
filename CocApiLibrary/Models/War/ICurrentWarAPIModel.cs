using System;
using System.Collections.Generic;

namespace CocApiLibrary.Models
{
    public interface ICurrentWarApiModel : IDownloadable
    {
        List<AttackApiModel> Attacks { get; set; }
        WarClanApiModel? Clan { get; set; }
        IList<WarClanApiModel> Clans { get; set; }
        DateTime EndTimeUtc { get; set; }
        CurrentWarFlagsModel Flags { get; }
        WarClanApiModel? Opponent { get; set; }
        DateTime PreparationStartTimeUtc { get; set; }
        DateTime StartTimeUtc { get; set; }
        Enums.WarState State { get; set; }
        int TeamSize { get; set; }
        DateTime WarEndingSoonUtc { get; }
        string WarId { get; set; }
        DateTime WarStartingSoonUtc { get; }
        Enums.WarType WarType { get; set; }

        bool WarIsOverOrAllAttacksUsed();


    }
}