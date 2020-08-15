using System;
using System.Collections.Generic;

namespace CocApi.Cache.Models.Wars
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