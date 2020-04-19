using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public interface IWarEvents
    {
        //Task LeagueGroupTeamSizeChangedAsync(LeagueGroup leagueGroup);
        Task NewAttacksAsync(CurrentWar currentWar, IReadOnlyList<Attack> attacks);
        Task NewWarAsync(CurrentWar currentWar);
        Task WarChangedAsync(CurrentWar oldWar, CurrentWar newWar);
        Task WarEndedAsync(CurrentWar currentWar);
        Task WarEndingSoonAsync(CurrentWar currentWar);
        Task WarEndNotSeenAsync(CurrentWar currentWar);
        Task WarEndSeenAsync(CurrentWar currentWar);
        Task WarIsAccessibleChangedAsync(CurrentWar currentWar, bool canRead);
        Task WarStartedAsync(CurrentWar currentWar);
        Task WarStartingSoonAsync(CurrentWar currentWar);
        Task InitialWarsDownloadedAsync(IReadOnlyList<CurrentWar> currentWars);
    }
}
