using System;
using System.Collections.Generic;
using CocApiLibrary.Models.Clan;

namespace CocApiLibrary.Models
{
    public interface ICurrentWarAPIModel : IDownloadable
    {
        List<AttackAPIModel> Attacks { get; set; }
        WarClanAPIModel? Clan { get; set; }
        IList<WarClanAPIModel> Clans { get; set; }
        DateTime EndTimeUtc { get; set; }
        CurrentWarFlags Flags { get; }
        WarClanAPIModel? Opponent { get; set; }
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