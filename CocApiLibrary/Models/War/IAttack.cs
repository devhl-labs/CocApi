using System;

namespace devhl.CocApi.Models.War
{
    public interface IAttack
    {
        string AttackerClanTag { get; }

        int AttackerMapPosition { get; }

        string AttackerTag { get; }

        int AttackerTownHallLevel { get; }

        string DefenderClanTag { get; }

        int? DefenderMapPosition { get; }

        string DefenderTag { get; }

        int? DefenderTownHallLevel { get; }

        int? DestructionPercentage { get; }

        bool Fresh { get; }

        int? Order { get; }

        DateTime DownloadedUtc { get; }

        int? Stars { get; }

        int? StarsGained { get; }
    }
}