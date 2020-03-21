using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public interface IVillageEvents
    {
        Task VillageAchievementsChangedAsync(Village village, IReadOnlyList<Achievement> achievements);
        Task VillageChangedAsync(Village oldVillage, Village newVillage);
        Task VillageDefenseWinsChangedAsync(Village village, int increase);
        Task VillageExpLevelChangedAsync(Village village, int increase);
        Task VillageHeroesChangedAsync(Village village, IReadOnlyList<Troop> troops);
        Task VillageLabelsChangedAsync(Village village, IReadOnlyList<VillageLabel> addedLabels, IReadOnlyList<VillageLabel> removedLabels);
        Task VillageReachedLegendsLeagueAsync(Village village);
        Task VillageSpellsChangedAsync(Village village, IReadOnlyList<Spell> spells);
        Task VillageTroopsChangedAsync(Village village, IReadOnlyList<Troop> troops);
        Task VillageTrophiesChangedAsync(Village village, int increase);
        Task VillageVersusBattleWinCountChangedAsync(Village village, int increase);
        Task VillageVersusBattleWinsChangedAsync(Village village, int increase);
        Task VillageVersusTrophiesChangedAsync(Village village, int increase);        
    }
}
