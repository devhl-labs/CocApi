using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;

using System.Collections.Immutable;

namespace devhl.CocApi
{
    public delegate Task VillageChangedEventHandler(Village oldVillage, Village newVillage);
    public delegate Task VillageDefenseWinsChangedEventHandler(Village village, int increase);
    public delegate Task VillageExpLevelChangedEventHandler(Village village, int increase);
    public delegate Task VillageTrophiesChangedEventHandler(Village village, int increase);
    public delegate Task VillageVersusBattleWinCountChangedEventHandler(Village village, int increase);
    public delegate Task VillageVersusBattleWinsChangedEventHandler(Village village, int increase);
    public delegate Task VillageVersusTrophiesChangedEventHandler(Village village, int increase);
    public delegate Task VillageAchievementsChangedEventHandler(Village village, IReadOnlyList<Achievement> achievements);
    public delegate Task VillageTroopsChangedEventHandler(Village village, IReadOnlyList<Troop> troops);
    public delegate Task VillageHeroesChangedEventHandler(Village village, IReadOnlyList<Troop> troops);
    public delegate Task VillageSpellsChangedEventHandler(Village village, IReadOnlyList<Spell> spells);
    public delegate Task VillageLabelsChangedEventHandler(Village village, IReadOnlyList<VillageLabel> addedLabels, IReadOnlyList<VillageLabel> removedLabels);
    public delegate Task VillageReachedLegendsLeagueEventHandler(Village village);


    public sealed partial class CocApi : IDisposable
    {
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///    <item><description><see cref="Village.AttackWins"/></description></item>
        ///    <item><description><see cref="Village.BestTrophies"/></description></item>
        ///    <item><description><see cref="Village.BestVersusTrophies"/></description></item>
        ///    <item><description><see cref="Village.BuilderHallLevel"/></description></item>
        ///    <item><description><see cref="Village.TownHallLevel"/></description></item>
        ///    <item><description><see cref="Village.TownHallWeaponLevel"/></description></item>
        ///    <item><description><see cref="Village.WarStars"/></description></item>
        /// </list>
        /// </summary>
        public event VillageChangedEventHandler? VillageChanged;
        public event VillageDefenseWinsChangedEventHandler? VillageDefenseWinsChanged;
        public event VillageExpLevelChangedEventHandler? VillageExpLevelChanged;
        public event VillageTrophiesChangedEventHandler? VillageTrophiesChanged;
        public event VillageVersusBattleWinCountChangedEventHandler? VillageVersusBattleWinCountChanged;
        public event VillageVersusBattleWinsChangedEventHandler? VillageVersusBattleWinsChanged;
        public event VillageVersusTrophiesChangedEventHandler? VillageVersusTrophiesChanged;
        public event VillageAchievementsChangedEventHandler? VillageAchievementsChanged;
        public event VillageTroopsChangedEventHandler? VillageTroopsChanged;
        public event VillageHeroesChangedEventHandler? VillageHeroesChanged;
        public event VillageSpellsChangedEventHandler? VillageSpellsChanged;
        public event VillageLabelsChangedEventHandler? VillageLabelsChanged;
        public event VillageReachedLegendsLeagueEventHandler? VillageReachedLegendsLeague;

        internal void VillageReachedLegendsLeagueEvent(Village village) => VillageReachedLegendsLeague?.Invoke(village);

        internal void VillageLabelsChangedEvent(Village village, List<VillageLabel> addedLabels, List<VillageLabel> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

            VillageLabelsChanged?.Invoke(village, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());
        }

        internal void VillageSpellsChangedEvent(Village village, List<Spell> spells) => VillageSpellsChanged?.Invoke(village, spells.ToImmutableArray());

        internal void VillageHeroesChangedEvent(Village village, List<Troop> heroes) => VillageHeroesChanged?.Invoke(village, heroes.ToImmutableArray());

        internal void VillageTroopsChangedEvent(Village village, List<Troop> troops) => VillageTroopsChanged?.Invoke(village, troops.ToImmutableArray());

        internal void VillageAchievementsChangedEvent(Village village, List<Achievement> achievements) => VillageAchievementsChanged?.Invoke(village, achievements.ToImmutableArray());

        internal void VillageVersusTrophiesChangedEvent(Village village, int increase) => VillageVersusTrophiesChanged?.Invoke(village, increase);

        internal void VillageVersusBattleWinsChangedEvent(Village village, int increase) => VillageVersusBattleWinsChanged?.Invoke(village, increase);

        internal void VillageVersusBattleWinCountChangedEvent(Village village, int increase) => VillageVersusBattleWinCountChanged?.Invoke(village, increase);

        internal void VillageTrophiesChangedEvent(Village village, int increase) => VillageTrophiesChanged?.Invoke(village, increase);

        internal void VillageExpLevelChangedEvent(Village village, int increase) => VillageExpLevelChanged?.Invoke(village, increase);

        internal void VillageDefenseWinsChangedEvent(Village village, int increase) => VillageDefenseWinsChanged?.Invoke(village, increase);

        internal void VillageChangedEvent(Village oldVillage, Village newVillage) => VillageChanged?.Invoke(oldVillage, newVillage);
    }
}
