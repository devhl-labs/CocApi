using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public interface IEventHandler
    {
        Task ClanBadgeUrlChangedAsync(Clan oldClan, Clan newClan);
        Task ClanChangedAsync(Clan oldClan, Clan newClan);
        Task ClanDonationsAsync(Clan oldClan, IReadOnlyList<Donation> receivedDonations, IReadOnlyList<Donation> gaveDonations);
        Task ClanDonationsResetAsync(Clan oldClan, Clan newClan);
        Task ClanLabelsChangedAsync(Clan newClanApiModel, IReadOnlyList<ClanLabel> addedLabels, IReadOnlyList<ClanLabel> removedLabels);
        Task ClanLocationChangedAsync(Clan oldClan, Clan newClan);
        Task ClanPointsChangedAsync(Clan oldClan, int newClanPoints);
        Task ClanVersusPointsChangedAsync(Clan oldClan, int newClanVersusPoints);
        Task ClanVillageNameChangedAsync(ClanVillage oldVillage, string newName);
        Task ClanVillagesLeagueChangedAsync(Clan oldClan, IReadOnlyList<LeagueChange> leagueChanged);
        Task ClanVillagesRoleChangedAsync(Clan clan, IReadOnlyList<RoleChange> roleChanges);
        Task ApiIsAvailableChangedAsync(bool isAvailable);
        Task LeagueGroupTeamSizeChangedAsync(LeagueGroup leagueGroupApiModel);
        Task NewAttacksAsync(CurrentWar currentWar, IReadOnlyList<Attack> newAttacks);
        Task NewWarAsync(CurrentWar currentWar);
        //Task NewWarsAsync(IReadOnlyList<CurrentWar> currentWar);
        Task VillageAchievementsChangedAsync(Village oldVillage, IReadOnlyList<Achievement> newAchievements);
        Task VillageChangedAsync(Village oldVillage, Village newVillage);
        Task VillageDefenseWinsChangedAsync(Village oldVillage, int newDefenseWins);
        Task VillageExpLevelChangedAsync(Village oldVillage, int newExpLevel);
        Task VillageHeroesChangedAsync(Village oldVillage, IReadOnlyList<Troop> newHeroes);
        Task VillageLabelsChangedAsync(Village newVillageApiModel, IReadOnlyList<VillageLabel> addedLabels, IReadOnlyList<VillageLabel> removedLabels);
        Task VillageReachedLegendsLeagueAsync(Village villageApiModel);
        Task VillagesJoinedAsync(Clan oldClan, IReadOnlyList<ClanVillage> villageListApiModels);
        Task VillagesLeftAsync(Clan oldClan, IReadOnlyList<ClanVillage> villageListApiModels);
        Task VillageSpellsChangedAsync(Village oldVillage, IReadOnlyList<Spell> newSpells);
        Task VillageTroopsChangedAsync(Village oldVillage, IReadOnlyList<Troop> newTroops);
        Task VillageTrophiesChangedAsync(Village oldVillage, int newTrophies);
        Task VillageVersusBattleWinCountChangedAsync(Village oldVillage, int newVersusBattleWinCount);
        Task VillageVersusBattleWinsChangedAsync(Village oldVillage, int newVersusBattleWins);
        Task VillageVersusTrophiesChangedAsync(Village oldVillage, int newVersusTrophies);
        Task WarChangedAsync(CurrentWar oldWar, CurrentWar newWar);
        Task WarEndedAsync(CurrentWar currentWar);
        Task WarEndingSoonAsync(CurrentWar currentWar);
        Task WarEndNotSeenAsync(CurrentWar currentWar);
        Task WarEndSeenAsync(CurrentWar currentWar);
        Task WarIsAccessibleChangedAsync(CurrentWar currentWar);
        Task WarStartedAsync(CurrentWar currentWar);
        Task WarStartingSoonAsync(CurrentWar currentWar);
        
    }
}
