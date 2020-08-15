using System.Collections.Generic;

namespace CocApi.Cache.Models.Wars
{
    public interface IWarVillage
    {
        string ClanTag { get; }

        int RosterPosition { get; }

        int TownHallLevel { get; }

        string VillageTag { get; }
    }
}