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
    public delegate Task ApiIsAvailableChangedEventHandler(bool isAvailable);
    public delegate Task ClanChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    public delegate Task VillagesJoinedEventHandler(ClanApiModel oldClan, IReadOnlyList<ClanVillageApiModel> villageListApiModels);
    public delegate Task VillagesLeftEventHandler(ClanApiModel oldClan, IReadOnlyList<ClanVillageApiModel> villageListApiModels);
    public delegate Task ClanBadgeUrlChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    public delegate Task ClanLocationChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    public delegate Task NewWarEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task WarChangedEventHandler(ICurrentWarApiModel oldWar, ICurrentWarApiModel newWar);
    public delegate Task NewAttacksEventHandler(ICurrentWarApiModel currentWarApiModel, IReadOnlyList<AttackApiModel> newAttacks);
    public delegate Task WarEndingSoonEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task WarStartingSoonEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task ClanVersusPointsChangedEventHandler(ClanApiModel oldClan, int newClanVersusPoints);
    public delegate Task ClanPointsChangedEventHandler(ClanApiModel oldClan, int newClanPoints);
    public delegate Task WarIsAccessibleChangedEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task WarEndNotSeenEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task VillageChangedEventHandler(VillageApiModel oldVillage, VillageApiModel newVillage);
    public delegate Task VillageDefenseWinsChangedEventHandler(VillageApiModel oldVillage, int newDefenseWins);
    public delegate Task VillageExpLevelChangedEventHandler(VillageApiModel oldVillage, int newExpLevel);
    public delegate Task VillageTrophiesChangedEventHandler(VillageApiModel oldVillage, int newTrophies);
    public delegate Task VillageVersusBattleWinCountChangedEventHandler(VillageApiModel oldVillage, int newVersusBattleWinCount);
    public delegate Task VillageVersusBattleWinsChangedEventHandler(VillageApiModel oldVillage, int newVersusBattleWins);
    public delegate Task VillageVersusTrophiesChangedEventHandler(VillageApiModel oldVillage, int newVersusTrophies);
    public delegate Task VillageAchievementsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<AchievementApiModel> newAchievements);
    public delegate Task VillageTroopsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<TroopApiModel> newTroops);
    public delegate Task VillageHeroesChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<TroopApiModel> newHeroes);
    public delegate Task VillageSpellsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<VillageSpellApiModel> newSpells);
    public delegate Task WarStartedEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task WarEndedEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task WarEndSeenEventHandler(ICurrentWarApiModel currentWarApiModel);
    public delegate Task LeagueGroupTeamSizeChangedEventHandler(LeagueGroupApiModel leagueGroupApiModel);
    public delegate Task ClanLabelsChangedEventHandler(ClanApiModel newClanApiModel, IReadOnlyList<ClanLabelApiModel> addedLabels, IReadOnlyList<ClanLabelApiModel> removedLables);
    public delegate Task VillageLabelsChangedEventHandler(VillageApiModel newVillageApiModel, IReadOnlyList<VillageLabelApiModel> addedLabels, IReadOnlyList<VillageLabelApiModel> removedLabels);
    public delegate Task VillageReachedLegendsLeagueEventHandler(VillageApiModel villageApiModel);
    public delegate Task ClanDonationsEventHandler(ClanApiModel oldClan, IReadOnlyList<Donation> receivedDonations, IReadOnlyList<Donation> gaveDonations);
    public delegate Task ClanVillageNameChangedEventHandler(ClanVillageApiModel oldVillage, string newName);
    public delegate Task ClanVillagesLeagueChangedEventHandler(ClanApiModel oldClan, IReadOnlyList<LeagueChange> leagueChanged);
    public delegate Task ClanVillagesRoleChangedEventHandler(ClanApiModel oldClan, IReadOnlyList<RoleChange> roleChanges);
    public delegate Task ClanDonationsResetEventHandler(ClanApiModel oldClan, ClanApiModel newClan);


    public sealed partial class CocApi : IDisposable
    {
        //private volatile bool _isAvailable = true;

        //private readonly List<UpdateService> _updateServices = new List<UpdateService>();

        //private readonly List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();

        //internal Dictionary<string, ClanApiModel> AllClans { get; } = new Dictionary<string, ClanApiModel>();

        //internal Dictionary<string, IWar> AllWarsByClanTag { get; } = new Dictionary<string, IWar>();

        //internal Dictionary<string, ICurrentWarApiModel> AllWarsByWarId { get; } = new Dictionary<string, ICurrentWarApiModel>();

        //internal Dictionary<string, LeagueWarApiModel> AllWarsByWarTag { get; } = new Dictionary<string, LeagueWarApiModel>();

        //internal Dictionary<string, ILeagueGroup> AllLeagueGroups { get; } = new Dictionary<string, ILeagueGroup>();

        //internal Dictionary<string, VillageApiModel> AllVillages { get; } = new Dictionary<string, VillageApiModel>();

        //internal CocApiConfiguration CocApiConfiguration { get; private set; } = new CocApiConfiguration();

        /// <summary>
        /// Fires if you query the Api during an outage.
        /// If the service is not available, you may still try to query the Api if you wish.
        /// </summary>
        public event ApiIsAvailableChangedEventHandler? ApiIsAvailableChanged;
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///    <item><description><see cref="ClanApiModel.ClanLevel"/></description></item>
        ///    <item><description><see cref="ClanApiModel.Description"/></description></item>
        ///    <item><description><see cref="ClanApiModel.IsWarLogPublic"/></description></item>
        ///    <item><description><see cref="ClanApiModel.Name"/></description></item>
        ///    <item><description><see cref="ClanApiModel.RequiredTrophies"/>RequiredTrophies</description></item>
        ///    <item><description><see cref="ClanApiModel.Recruitment"/></description></item>
        ///    <item><description><see cref="ClanApiModel.VillageCount"/></description></item>
        ///    <item><description><see cref="ClanApiModel.WarLosses"/></description></item>
        ///    <item><description><see cref="ClanApiModel.WarWins"/></description></item>
        ///    <item><description><see cref="ClanApiModel.Wars"/></description></item>
        ///    <item><description><see cref="ClanApiModel.WarTies"/></description></item>
        ///    <item><description><see cref="ClanApiModel.WarFrequency"/></description></item>
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
        ///     <item><description><see cref="ICurrentWarApiModel.EndTimeUtc"/></description></item>
        ///     <item><description><see cref="ICurrentWarApiModel.StartTimeUtc"/></description></item>
        ///     <item><description><see cref="ICurrentWarApiModel.State"/></description></item>
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
        ///    <item><description><see cref="VillageApiModel.AttackWins"/></description></item>
        ///    <item><description><see cref="VillageApiModel.BestTrophies"/></description></item>
        ///    <item><description><see cref="VillageApiModel.BestVersusTrophies"/></description></item>
        ///    <item><description><see cref="VillageApiModel.BuilderHallLevel"/></description></item>
        ///    <item><description><see cref="VillageApiModel.TownHallLevel"/></description></item>
        ///    <item><description><see cref="VillageApiModel.TownHallWeaponLevel"/></description></item>
        ///    <item><description><see cref="VillageApiModel.WarStars"/></description></item>
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
        /// Fires when the <see cref="ICurrentWarApiModel.EndTimeUtc"/> has elapsed.  The Api may or may not show the war end when this event occurs.
        /// </summary>
        public event WarEndedEventHandler? WarEnded;
        /// <summary>
        /// Fires when the Api shows <see cref="ICurrentWarApiModel.State"/> is <see cref="Enums.WarState.WarEnded"/>
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

        internal void ClanDonationsResetEvent(ClanApiModel oldClan, ClanApiModel newClan)
        {
            ClanDonationsReset?.Invoke(oldClan, newClan);
        }

        internal void ClanVillagesRoleChangedEvent(ClanApiModel clan, List<RoleChange> roleChanges)
        {
            if (roleChanges.Count > 0)
            {

                ClanVillagesRoleChanged?.Invoke(clan, roleChanges.ToImmutableArray());
            }
        }

        internal void ClanVillagesLeagueChangedEvent(ClanApiModel oldClan, List<LeagueChange> leagueChanged)
        {
            if (leagueChanged.Count > 0)
            {
                ClanVillagesLeagueChanged?.Invoke(oldClan, leagueChanged.ToImmutableArray());

            }
        }

        internal void ClanVillageNameChangedEvent(ClanVillageApiModel oldVillage, string newName)
        {
            ClanVillageNameChanged?.Invoke(oldVillage, newName);
        }

        internal void ClanDonationsEvent(ClanApiModel oldClan, List<Donation> received, List<Donation> donated)
        {
            if (received.Count > 0 || donated.Count > 0)
            {
                ClanDonations?.Invoke(oldClan, received.ToImmutableArray(), donated.ToImmutableArray());
            }
        }

        internal void VillageReachedLegendsLeagueEvent(VillageApiModel villageApiModel)
        {
            VillageReachedLegendsLeague?.Invoke(villageApiModel);
        }

        internal void ClanLabelsChangedEvent(ClanApiModel newClan, List<ClanLabelApiModel> addedLabels, List<ClanLabelApiModel> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

            ClanLabelsChanged?.Invoke(newClan, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());
        }

        internal void VillageLabelsChangedEvent(VillageApiModel newVillage, List<VillageLabelApiModel> addedLabels, List<VillageLabelApiModel> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

            VillageLabelsChanged?.Invoke(newVillage, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());

        }

        internal void LeagueGroupTeamSizeChangedEvent(LeagueGroupApiModel leagueGroupApiModel)
        {
            LeagueGroupTeamSizeChanged?.Invoke(leagueGroupApiModel);
        }

        internal void WarEndSeenEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarEndSeen?.Invoke(currentWarApiModel);
        }

        internal void WarEndedEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarEnded?.Invoke(currentWarApiModel);
        }

        internal void WarStartedEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarStarted?.Invoke(currentWarApiModel);
        }

        internal void VillageSpellsChangedEvent(VillageApiModel oldVillage, List<VillageSpellApiModel> newSpells)
        {
            VillageSpellsChanged?.Invoke(oldVillage, newSpells.ToImmutableArray());
        }

        internal void VillageHeroesChangedEvent(VillageApiModel oldVillage, List<TroopApiModel> newHeroes)
        {
            VillageHeroesChanged?.Invoke(oldVillage, newHeroes.ToImmutableArray());
        }

        internal void VillageTroopsChangedEvent(VillageApiModel oldVillage, List<TroopApiModel> newTroops)
        {
            VillageTroopsChanged?.Invoke(oldVillage, newTroops.ToImmutableArray());
        }

        internal void VillageAchievementsChangedEvent(VillageApiModel oldVillage, List<AchievementApiModel> newAchievements)
        {
            VillageAchievementsChanged?.Invoke(oldVillage, newAchievements.ToImmutableArray());
        }

        internal void VillageVersusTrophiesChangedEvent(VillageApiModel oldVillage, int newVersusTrophies)
        {
            VillageVersusTrophiesChanged?.Invoke(oldVillage, newVersusTrophies);
        }

        internal void VillageVersusBattleWinsChangedEvent(VillageApiModel oldVillage, int newVersusBattleWins)
        {
            VillageVersusBattleWinsChanged?.Invoke(oldVillage, newVersusBattleWins);
        }

        internal void VillageVersusBattleWinCountChangedEvent(VillageApiModel oldVillage, int newVersusBattleWinCount)
        {
            VillageVersusBattleWinCountChanged?.Invoke(oldVillage, newVersusBattleWinCount);
        }

        internal void VillageTrophiesChangedEvent(VillageApiModel oldVillage, int newTrophies)
        {
            VillageTrophiesChanged?.Invoke(oldVillage, newTrophies);
        }

        internal void VillageExpLevelChangedEvent(VillageApiModel oldVillage, int newExpLevel)
        {
            VillageExpLevelChanged?.Invoke(oldVillage, newExpLevel);
        }

        internal void VillageDefenseWinsChangedEvent(VillageApiModel oldVillage, int newDefenseWinsChanged)
        {
            VillageDefenseWinsChanged?.Invoke(oldVillage, newDefenseWinsChanged);
        }

        internal void VillageChangedEvent(VillageApiModel oldVillage, VillageApiModel newVillage)
        {
            VillageChanged?.Invoke(oldVillage, newVillage);
        }

        internal void WarEndNotSeenEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarEndNotSeen?.Invoke(currentWarApiModel);
        }

        internal void WarIsAccessibleChangedEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarIsAccessibleChanged?.Invoke(currentWarApiModel);
        }

        internal void ClanPointsChangedEvent(ClanApiModel oldClan, int newClanPoints)
        {
            ClanPointsChanged?.Invoke(oldClan, newClanPoints);
        }

        internal void ClanVersusPointsChangedEvent(ClanApiModel oldClan, int newClanVersusPoints)
        {
            ClanVersusPointsChanged?.Invoke(oldClan, newClanVersusPoints);
        }

        internal void WarStartingSoonEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarStartingSoon?.Invoke(currentWarApiModel);
        }

        internal void WarEndingSoonEvent(ICurrentWarApiModel currentWarApiModel)
        {
            WarEndingSoon?.Invoke(currentWarApiModel);
        }

        internal void NewAttacksEvent(ICurrentWarApiModel currentWarApiModel, List<AttackApiModel> attackApiModels)
        {
            if (attackApiModels.Count > 0)
            {
                NewAttacks?.Invoke(currentWarApiModel, attackApiModels.ToImmutableArray());
            }
        }

        internal void WarChangedEvent(ICurrentWarApiModel oldWar, ICurrentWarApiModel newWar)
        {
            WarChanged?.Invoke(oldWar, newWar);
        }

        internal void NewWarEvent(ICurrentWarApiModel currentWarApiModel)
        {
            NewWar?.Invoke(currentWarApiModel);
        }

        internal void VillagesLeftEvent(ClanApiModel newClan, List<ClanVillageApiModel> clanVillageApiModels)
        {
            if (clanVillageApiModels.Count > 0)
            {
                VillagesLeft?.Invoke(newClan, clanVillageApiModels.ToImmutableArray());
            }            
        }

        internal void ClanLocationChangedEvent(ClanApiModel oldClan, ClanApiModel newClan)
        {
            ClanLocationChanged?.Invoke(oldClan, newClan);
        }

        internal void ClanBadgeUrlChangedEvent(ClanApiModel oldClan, ClanApiModel newClan)
        {
            ClanBadgeUrlChanged?.Invoke(oldClan, newClan);
        }

        internal void ClanChangedEvent(ClanApiModel oldClan, ClanApiModel newClan)
        {
            ClanChanged?.Invoke(oldClan, newClan);
        }

        internal void VillagesJoinedEvent(ClanApiModel newClan, List<ClanVillageApiModel> clanVillageApiModels)
        {
            if (clanVillageApiModels.Count > 0)
            {
                VillagesJoined?.Invoke(newClan, clanVillageApiModels.ToImmutableArray());
            }
        }
    }
}
