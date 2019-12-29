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

using System.Collections.Immutable;
using System.Collections.Concurrent;

namespace devhl.CocApi
{
    public delegate Task ApiIsAvailableChangedEventHandler(bool isAvailable);
    public delegate Task ClanChangedEventHandler(Clan oldClan, Clan newClan);
    public delegate Task VillagesJoinedEventHandler(Clan oldClan, IReadOnlyList<ClanVillage> villageListApiModels);
    public delegate Task VillagesLeftEventHandler(Clan oldClan, IReadOnlyList<ClanVillage> villageListApiModels);
    public delegate Task ClanBadgeUrlChangedEventHandler(Clan oldClan, Clan newClan);
    public delegate Task ClanLocationChangedEventHandler(Clan oldClan, Clan newClan);
    public delegate Task NewWarEventHandler(IActiveWar currentWarApiModel);
    public delegate Task WarChangedEventHandler(IActiveWar oldWar, IActiveWar newWar);
    public delegate Task NewAttacksEventHandler(IActiveWar currentWarApiModel, IReadOnlyList<Attack> newAttacks);
    public delegate Task WarEndingSoonEventHandler(IActiveWar currentWarApiModel);
    public delegate Task WarStartingSoonEventHandler(IActiveWar currentWarApiModel);
    public delegate Task ClanVersusPointsChangedEventHandler(Clan oldClan, int newClanVersusPoints);
    public delegate Task ClanPointsChangedEventHandler(Clan oldClan, int newClanPoints);
    public delegate Task WarIsAccessibleChangedEventHandler(IActiveWar currentWarApiModel);
    public delegate Task WarEndNotSeenEventHandler(IActiveWar currentWarApiModel);
    public delegate Task VillageChangedEventHandler(Village oldVillage, Village newVillage);
    public delegate Task VillageDefenseWinsChangedEventHandler(Village oldVillage, int newDefenseWins);
    public delegate Task VillageExpLevelChangedEventHandler(Village oldVillage, int newExpLevel);
    public delegate Task VillageTrophiesChangedEventHandler(Village oldVillage, int newTrophies);
    public delegate Task VillageVersusBattleWinCountChangedEventHandler(Village oldVillage, int newVersusBattleWinCount);
    public delegate Task VillageVersusBattleWinsChangedEventHandler(Village oldVillage, int newVersusBattleWins);
    public delegate Task VillageVersusTrophiesChangedEventHandler(Village oldVillage, int newVersusTrophies);
    public delegate Task VillageAchievementsChangedEventHandler(Village oldVillage, IReadOnlyList<Achievement> newAchievements);
    public delegate Task VillageTroopsChangedEventHandler(Village oldVillage, IReadOnlyList<Troop> newTroops);
    public delegate Task VillageHeroesChangedEventHandler(Village oldVillage, IReadOnlyList<Troop> newHeroes);
    public delegate Task VillageSpellsChangedEventHandler(Village oldVillage, IReadOnlyList<VillageSpell> newSpells);
    public delegate Task WarStartedEventHandler(IActiveWar currentWarApiModel);
    public delegate Task WarEndedEventHandler(IActiveWar currentWarApiModel);
    public delegate Task WarEndSeenEventHandler(IActiveWar currentWarApiModel);
    public delegate Task LeagueGroupTeamSizeChangedEventHandler(LeagueGroup leagueGroupApiModel);
    public delegate Task ClanLabelsChangedEventHandler(Clan newClanApiModel, IReadOnlyList<ClanLabel> addedLabels, IReadOnlyList<ClanLabel> removedLables);
    public delegate Task VillageLabelsChangedEventHandler(Village newVillageApiModel, IReadOnlyList<VillageLabel> addedLabels, IReadOnlyList<VillageLabel> removedLabels);
    public delegate Task VillageReachedLegendsLeagueEventHandler(Village villageApiModel);
    public delegate Task ClanDonationsEventHandler(Clan oldClan, IReadOnlyList<Donation> receivedDonations, IReadOnlyList<Donation> gaveDonations);
    public delegate Task ClanVillageNameChangedEventHandler(ClanVillage oldVillage, string newName);
    public delegate Task ClanVillagesLeagueChangedEventHandler(Clan oldClan, IReadOnlyList<LeagueChange> leagueChanged);
    public delegate Task ClanVillagesRoleChangedEventHandler(Clan oldClan, IReadOnlyList<RoleChange> roleChanges);
    public delegate Task ClanDonationsResetEventHandler(Clan oldClan, Clan newClan);


    public sealed partial class CocApi : IDisposable
    {
        /// <summary>
        /// Fires if you query the Api during an outage.
        /// If the service is not available, you may still try to query the Api if you wish.
        /// </summary>
        public event ApiIsAvailableChangedEventHandler? ApiIsAvailableChanged;
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///    <item><description><see cref="Clan.ClanLevel"/></description></item>
        ///    <item><description><see cref="Clan.Description"/></description></item>
        ///    <item><description><see cref="Clan.IsWarLogPublic"/></description></item>
        ///    <item><description><see cref="Clan.Name"/></description></item>
        ///    <item><description><see cref="Clan.RequiredTrophies"/>RequiredTrophies</description></item>
        ///    <item><description><see cref="Clan.Recruitment"/></description></item>
        ///    <item><description><see cref="Clan.VillageCount"/></description></item>
        ///    <item><description><see cref="Clan.WarLosses"/></description></item>
        ///    <item><description><see cref="Clan.WarWins"/></description></item>
        ///    <item><description><see cref="Clan.Wars"/></description></item>
        ///    <item><description><see cref="Clan.WarTies"/></description></item>
        ///    <item><description><see cref="Clan.WarFrequency"/></description></item>
        /// </list>
        /// </summary>
        public event ClanChangedEventHandler? ClanChanged;
        public event VillagesJoinedEventHandler? VillagesJoined;
        public event VillagesLeftEventHandler? VillagesLeft;
        public event ClanBadgeUrlChangedEventHandler? ClanBadgeUrlChanged;
        public event ClanLocationChangedEventHandler? ClanLocationChanged;
        public event NewWarEventHandler? NewWar;
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///     <item><description><see cref="IActiveWar.EndTimeUtc"/></description></item>
        ///     <item><description><see cref="IActiveWar.StartTimeUtc"/></description></item>
        ///     <item><description><see cref="IActiveWar.State"/></description></item>
        /// 
        /// </list>
        /// </summary>
        public event WarChangedEventHandler? WarChanged;
        public event NewAttacksEventHandler? NewAttacks;
        public event WarEndingSoonEventHandler? WarEndingSoon;
        public event WarStartingSoonEventHandler? WarStartingSoon;
        public event ClanVersusPointsChangedEventHandler? ClanVersusPointsChanged;
        public event ClanPointsChangedEventHandler? ClanPointsChanged;

        /// <summary>
        /// Fires when the donations decrease.
        /// </summary>
        public event ClanDonationsResetEventHandler? ClanDonationsReset;


        /// <summary>
        /// Fires if the war cannot be found from either clanTags or warTag.  Private war logs can also fire this.
        /// </summary>
        public event WarIsAccessibleChangedEventHandler? WarIsAccessibleChanged;
        /// <summary>
        /// Fires when the war is not accessible and the end time has passed.  
        /// This war may still become available if one of the clans does not spin and opens their war log.
        /// </summary>
        public event WarEndNotSeenEventHandler? WarEndNotSeen;
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
        //public event VillageDonationsChangedEventHandler? VillageDonationsChanged;
        //public event VillageDonationsReceivedChangedEventHandler? VillageDonationsReceivedChanged;
        public event VillageExpLevelChangedEventHandler? VillageExpLevelChanged;
        public event VillageTrophiesChangedEventHandler? VillageTrophiesChanged;
        public event VillageVersusBattleWinCountChangedEventHandler? VillageVersusBattleWinCountChanged;
        public event VillageVersusBattleWinsChangedEventHandler? VillageVersusBattleWinsChanged;
        public event VillageVersusTrophiesChangedEventHandler? VillageVersusTrophiesChanged;
        public event VillageAchievementsChangedEventHandler? VillageAchievementsChanged;
        public event VillageTroopsChangedEventHandler? VillageTroopsChanged;
        public event VillageHeroesChangedEventHandler? VillageHeroesChanged;
        public event VillageSpellsChangedEventHandler? VillageSpellsChanged;
        public event WarStartedEventHandler? WarStarted;
        /// <summary>
        /// Fires when the <see cref="IActiveWar.EndTimeUtc"/> has elapsed.  The Api may or may not show the war end when this event occurs.
        /// </summary>
        public event WarEndedEventHandler? WarEnded;
        /// <summary>
        /// Fires when the Api shows <see cref="IActiveWar.State"/> is <see cref="Enums.WarState.WarEnded"/>
        /// </summary>
        public event WarEndSeenEventHandler? WarEndSeen;
        /// <summary>
        /// Fires when any clan in a league group has more than 15 attacks.
        /// </summary>
        public event LeagueGroupTeamSizeChangedEventHandler? LeagueGroupTeamSizeChanged;
        public event ClanLabelsChangedEventHandler? ClanLabelsChanged;
        public event VillageLabelsChangedEventHandler? VillageLabelsChanged;
        public event VillageReachedLegendsLeagueEventHandler? VillageReachedLegendsLeague;
        public event ClanDonationsEventHandler? ClanDonations;
        public event ClanVillageNameChangedEventHandler? ClanVillageNameChanged;
        public event ClanVillagesLeagueChangedEventHandler? ClanVillagesLeagueChanged;
        public event ClanVillagesRoleChangedEventHandler? ClanVillagesRoleChanged;
        /// <summary>
        /// Fires when an update task encounters an error.  Recommended fix action is to <see cref="StartUpdatingClans()"/> or restart.
        /// </summary>


        internal void CrashDetectedEvent()
        {
            try
            {
                Task.Run(async () =>
                {
                    //wait to allow the updater to finish crashing
                    await Task.Delay(5000).ConfigureAwait(false);

                    StartUpdatingClans();

                    Logger.LogInformation(LoggingEvents.None, "{source} Update services restarted.", _source);
                });    
            }
            catch (Exception e)
            { 
                Logger.LogWarning(LoggingEvents.UnhandledError, "{source} {message}", _source, e.Message);
            }
        }

        internal void ClanDonationsResetEvent(Clan oldClan, Clan newClan) => ClanDonationsReset?.Invoke(oldClan, newClan);

        internal void ClanVillagesRoleChangedEvent(Clan clan, List<RoleChange> roleChanges)
        {
            if (roleChanges.Count > 0)
            {
                ClanVillagesRoleChanged?.Invoke(clan, roleChanges.ToImmutableArray());
            }
        }

        internal void ClanVillagesLeagueChangedEvent(Clan oldClan, List<LeagueChange> leagueChanged)
        {
            if (leagueChanged.Count > 0)
            {
                ClanVillagesLeagueChanged?.Invoke(oldClan, leagueChanged.ToImmutableArray());
            }
        }

        internal void ClanVillageNameChangedEvent(ClanVillage oldVillage, string newName) => ClanVillageNameChanged?.Invoke(oldVillage, newName);

        internal void ClanDonationsEvent(Clan oldClan, List<Donation> received, List<Donation> donated)
        {
            if (received.Count > 0 || donated.Count > 0)
            {
                ClanDonations?.Invoke(oldClan, received.ToImmutableArray(), donated.ToImmutableArray());
            }
        }

        internal void VillageReachedLegendsLeagueEvent(Village villageApiModel) => VillageReachedLegendsLeague?.Invoke(villageApiModel);

        internal void ClanLabelsChangedEvent(Clan newClan, List<ClanLabel> addedLabels, List<ClanLabel> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

            ClanLabelsChanged?.Invoke(newClan, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());
        }

        internal void VillageLabelsChangedEvent(Village newVillage, List<VillageLabel> addedLabels, List<VillageLabel> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

            VillageLabelsChanged?.Invoke(newVillage, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());
        }

        internal void LeagueGroupTeamSizeChangedEvent(LeagueGroup leagueGroupApiModel) => LeagueGroupTeamSizeChanged?.Invoke(leagueGroupApiModel);

        internal void WarEndSeenEvent(IActiveWar currentWarApiModel) => WarEndSeen?.Invoke(currentWarApiModel);

        internal void WarEndedEvent(IActiveWar currentWarApiModel) => WarEnded?.Invoke(currentWarApiModel);

        internal void WarStartedEvent(IActiveWar currentWarApiModel) => WarStarted?.Invoke(currentWarApiModel);

        internal void VillageSpellsChangedEvent(Village oldVillage, List<VillageSpell> newSpells) => VillageSpellsChanged?.Invoke(oldVillage, newSpells.ToImmutableArray());

        internal void VillageHeroesChangedEvent(Village oldVillage, List<Troop> newHeroes) => VillageHeroesChanged?.Invoke(oldVillage, newHeroes.ToImmutableArray());

        internal void VillageTroopsChangedEvent(Village oldVillage, List<Troop> newTroops) => VillageTroopsChanged?.Invoke(oldVillage, newTroops.ToImmutableArray());

        internal void VillageAchievementsChangedEvent(Village oldVillage, List<Achievement> newAchievements) => VillageAchievementsChanged?.Invoke(oldVillage, newAchievements.ToImmutableArray());

        internal void VillageVersusTrophiesChangedEvent(Village oldVillage, int newVersusTrophies) => VillageVersusTrophiesChanged?.Invoke(oldVillage, newVersusTrophies);

        internal void VillageVersusBattleWinsChangedEvent(Village oldVillage, int newVersusBattleWins) => VillageVersusBattleWinsChanged?.Invoke(oldVillage, newVersusBattleWins);

        internal void VillageVersusBattleWinCountChangedEvent(Village oldVillage, int newVersusBattleWinCount) => VillageVersusBattleWinCountChanged?.Invoke(oldVillage, newVersusBattleWinCount);

        internal void VillageTrophiesChangedEvent(Village oldVillage, int newTrophies) => VillageTrophiesChanged?.Invoke(oldVillage, newTrophies);

        internal void VillageExpLevelChangedEvent(Village oldVillage, int newExpLevel) => VillageExpLevelChanged?.Invoke(oldVillage, newExpLevel);

        internal void VillageDefenseWinsChangedEvent(Village oldVillage, int newDefenseWinsChanged) => VillageDefenseWinsChanged?.Invoke(oldVillage, newDefenseWinsChanged);

        internal void VillageChangedEvent(Village oldVillage, Village newVillage) => VillageChanged?.Invoke(oldVillage, newVillage);

        internal void WarEndNotSeenEvent(IActiveWar currentWarApiModel) => WarEndNotSeen?.Invoke(currentWarApiModel);

        internal void WarIsAccessibleChangedEvent(IActiveWar currentWarApiModel) => WarIsAccessibleChanged?.Invoke(currentWarApiModel);

        internal void ClanPointsChangedEvent(Clan oldClan, int newClanPoints) => ClanPointsChanged?.Invoke(oldClan, newClanPoints);

        internal void ClanVersusPointsChangedEvent(Clan oldClan, int newClanVersusPoints) => ClanVersusPointsChanged?.Invoke(oldClan, newClanVersusPoints);

        internal void WarStartingSoonEvent(IActiveWar currentWarApiModel) => WarStartingSoon?.Invoke(currentWarApiModel);

        internal void WarEndingSoonEvent(IActiveWar currentWarApiModel) => WarEndingSoon?.Invoke(currentWarApiModel);

        internal void NewAttacksEvent(IActiveWar currentWarApiModel, List<Attack> attackApiModels)
        {
            if (attackApiModels.Count > 0)
            {
                NewAttacks?.Invoke(currentWarApiModel, attackApiModels.ToImmutableArray());
            }
        }

        internal void WarChangedEvent(IActiveWar oldWar, IActiveWar newWar) => WarChanged?.Invoke(oldWar, newWar);

        internal void NewWarEvent(IActiveWar currentWarApiModel) => NewWar?.Invoke(currentWarApiModel);

        internal void VillagesLeftEvent(Clan newClan, List<ClanVillage> clanVillageApiModels)
        {
            if (clanVillageApiModels.Count > 0)
            {
                VillagesLeft?.Invoke(newClan, clanVillageApiModels.ToImmutableArray());
            }            
        }

        internal void ClanLocationChangedEvent(Clan oldClan, Clan newClan) => ClanLocationChanged?.Invoke(oldClan, newClan);

        internal void ClanBadgeUrlChangedEvent(Clan oldClan, Clan newClan) => ClanBadgeUrlChanged?.Invoke(oldClan, newClan);

        internal void ClanChangedEvent(Clan oldClan, Clan newClan) => ClanChanged?.Invoke(oldClan, newClan);

        internal void VillagesJoinedEvent(Clan newClan, List<ClanVillage> clanVillageApiModels)
        {
            if (clanVillageApiModels.Count > 0)
            {
                VillagesJoined?.Invoke(newClan, clanVillageApiModels.ToImmutableArray());
            }
        }
    }
}
