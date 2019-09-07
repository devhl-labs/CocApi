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
        DateTime EndTimeUTC { get; set; }
        CurrentWarFlags Flags { get; }
        WarClanAPIModel? Opponent { get; set; }
        DateTime PreparationStartTimeUTC { get; set; }
        DateTime StartTimeUTC { get; set; }
        Enums.WarState State { get; set; }
        int TeamSize { get; set; }
        DateTime WarEndingSoonUTC { get; }
        string WarID { get; set; }
        DateTime WarStartingSoonUTC { get; }
        Enums.WarType WarType { get; set; }

        bool WarIsOverOrAllAttacksUsed();


    }
}