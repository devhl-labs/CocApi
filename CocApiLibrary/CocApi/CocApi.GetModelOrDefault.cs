using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

using devhl.CocApi.Models;
using devhl.CocApi.Exceptions;
using static devhl.CocApi.Enums;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using devhl.CocApi.Models.Location;
using System.Collections.Immutable;
using System.Collections.Concurrent;

namespace devhl.CocApi
{
    //public delegate Task ApiIsAvailableChangedEventHandler(bool isAvailable);
    //public delegate Task ClanChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    //public delegate Task VillagesJoinedEventHandler(ClanApiModel oldClan, IReadOnlyList<ClanVillageApiModel> villageListApiModels);
    //public delegate Task VillagesLeftEventHandler(ClanApiModel oldClan, IReadOnlyList<ClanVillageApiModel> villageListApiModels);
    //public delegate Task ClanBadgeUrlChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    //public delegate Task ClanLocationChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    //public delegate Task NewWarEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task WarChangedEventHandler(ICurrentWarApiModel oldWar, ICurrentWarApiModel newWar);
    //public delegate Task NewAttacksEventHandler(ICurrentWarApiModel currentWarApiModel, IReadOnlyList<AttackApiModel> newAttacks);
    //public delegate Task WarEndingSoonEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task WarStartingSoonEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task ClanVersusPointsChangedEventHandler(ClanApiModel oldClan, int newClanVersusPoints);
    //public delegate Task ClanPointsChangedEventHandler(ClanApiModel oldClan, int newClanPoints);
    //public delegate Task WarIsAccessibleChangedEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task WarEndNotSeenEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task VillageChangedEventHandler(VillageApiModel oldVillage, VillageApiModel newVillage);
    //public delegate Task VillageDefenseWinsChangedEventHandler(VillageApiModel oldVillage, int newDefenseWins);
    //public delegate Task VillageExpLevelChangedEventHandler(VillageApiModel oldVillage, int newExpLevel);
    //public delegate Task VillageTrophiesChangedEventHandler(VillageApiModel oldVillage, int newTrophies);
    //public delegate Task VillageVersusBattleWinCountChangedEventHandler(VillageApiModel oldVillage, int newVersusBattleWinCount);
    //public delegate Task VillageVersusBattleWinsChangedEventHandler(VillageApiModel oldVillage, int newVersusBattleWins);
    //public delegate Task VillageVersusTrophiesChangedEventHandler(VillageApiModel oldVillage, int newVersusTrophies);
    //public delegate Task VillageAchievementsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<AchievementApiModel> newAchievements);
    //public delegate Task VillageTroopsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<TroopApiModel> newTroops);
    //public delegate Task VillageHeroesChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<TroopApiModel> newHeroes);
    //public delegate Task VillageSpellsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<VillageSpellApiModel> newSpells);
    //public delegate Task WarStartedEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task WarEndedEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task WarEndSeenEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task LeagueGroupTeamSizeChangedEventHandler(LeagueGroupApiModel leagueGroupApiModel);
    //public delegate Task ClanLabelsChangedEventHandler(ClanApiModel newClanApiModel, IReadOnlyList<ClanLabelApiModel> addedLabels, IReadOnlyList<ClanLabelApiModel> removedLables);
    //public delegate Task VillageLabelsChangedEventHandler(VillageApiModel newVillageApiModel, IReadOnlyList<VillageLabelApiModel> addedLabels, IReadOnlyList<VillageLabelApiModel> removedLabels);
    //public delegate Task VillageReachedLegendsLeagueEventHandler(VillageApiModel villageApiModel);
    //public delegate Task ClanDonationsEventHandler(ClanApiModel oldClan, IReadOnlyList<Donation> receivedDonations, IReadOnlyList<Donation> gaveDonations);
    //public delegate Task ClanVillageNameChangedEventHandler(ClanVillageApiModel oldVillage, string newName);
    //public delegate Task ClanVillagesLeagueChangedEventHandler(ClanApiModel oldClan, IReadOnlyList<LeagueChange> leagueChanged);
    //public delegate Task ClanVillagesRoleChangedEventHandler(ClanApiModel oldClan, IReadOnlyList<RoleChange> roleChanges);
    //public delegate Task ClanDonationsResetEventHandler(ClanApiModel oldClan, ClanApiModel newClan);


    public sealed partial class CocApi : IDisposable
    {
        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the clanTag is not found.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public ClanApiModel? GetClanOrDefault(string clanTag)
        {
            AllClans.TryGetValue(clanTag, out ClanApiModel? result, AllClans);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the war log is private.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public IWar? GetWarByClanTagOrDefault(string clanTag)
        {
            AllWarsByClanTag.TryGetValue(clanTag, out IWar? result, AllWarsByClanTag);

            return result;
        }

        public ICurrentWarApiModel? GetWarByWarIdOrDefault(string warId)
        {
            AllWarsByWarId.TryGetValue(warId, out ICurrentWarApiModel? currentWar, AllWarsByWarId);

            return currentWar;
        }
 

        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the clan is not in a clan is not found to be in a league.  
        /// Returns <see cref="LeagueGroupApiModel"/> or <see cref="LeagueGroupNotFound"/>
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public ILeagueGroup? GetLeagueGroupOrDefault(string clanTag)
        {
            AllLeagueGroups.TryGetValue(clanTag, out ILeagueGroup? result, AllLeagueGroups);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the warTag is not found.
        /// </summary>
        /// <param name="warTag"></param>
        /// <returns></returns>
        public LeagueWarApiModel? GetLeagueWarOrDefault(string warTag)
        {
            AllWarsByWarTag.TryGetValue(warTag, out LeagueWarApiModel? result, AllWarsByWarTag);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the villageTag is not found.
        /// </summary>
        /// <param name="villageTag"></param>
        /// <returns></returns>
        public VillageApiModel? GetVillageOrDefault(string villageTag)
        {            
            AllVillages.TryGetValue(villageTag, out VillageApiModel? result, AllVillages);

            return result;
        }
    }
}
