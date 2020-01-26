using System;
using System.Collections.Generic;

namespace devhl.CocApi.Models.War
{
    public interface IActiveWar : IDownloadable, IWar
    {
        List<Attack> Attacks { get;  }

        IList<WarClan> Clans { get; }

        DateTime EndTimeUtc { get; }

        CurrentWarFlags Flags { get; }

        DateTime PreparationStartTimeUtc { get; }

        DateTime StartTimeUtc { get; }

        WarState State { get; }

        int TeamSize { get; }

        DateTime WarEndingSoonUtc { get; }

        string WarId { get; }

        DateTime WarStartingSoonUtc { get; }

        WarType WarType { get; }

        bool WarIsOverOrAllAttacksUsed();
    }
}