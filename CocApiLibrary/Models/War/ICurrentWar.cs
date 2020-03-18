using System;
using System.Collections.Generic;

namespace devhl.CocApi.Models.War
{
    public interface ICurrentWar
    {
        DateTime EndTimeUtc { get; }

        DateTime PreparationStartTimeUtc { get; }

        DateTime StartTimeUtc { get; }

        WarState State { get; }

        int TeamSize { get; }

        WarType WarType { get; }
    }
}