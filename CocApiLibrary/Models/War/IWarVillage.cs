﻿using System.Collections.Generic;

namespace devhl.CocApi.Models.War
{
    public interface IWarVillage
    {
        string ClanTag { get; }

        int RosterPosition { get; }

        int TownHallLevel { get; }

        string VillageTag { get; }
    }
}