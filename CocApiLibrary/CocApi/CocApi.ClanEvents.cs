using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;

using System.Collections.Immutable;

namespace devhl.CocApi
{
    public delegate Task ApiIsAvailableChangedEventHandler(bool isAvailable);
    public delegate Task ClanChangedEventHandler(Clan oldClan, Clan newClan);
    public delegate Task VillagesJoinedEventHandler(Clan clan, IReadOnlyList<ClanVillage> clanVillages);
    public delegate Task VillagesLeftEventHandler(Clan clan, IReadOnlyList<ClanVillage> clanVillages);
    public delegate Task ClanBadgeUrlChangedEventHandler(Clan oldClan, Clan newClan);
    public delegate Task ClanLocationChangedEventHandler(Clan oldClan, Clan newClan);
    public delegate Task NewWarEventHandler(CurrentWar currentWar);
    public delegate Task WarChangedEventHandler(CurrentWar oldWar, CurrentWar newWar);
    public delegate Task NewAttacksEventHandler(CurrentWar currentWar, IReadOnlyList<Attack> newAttacks);
    public delegate Task WarEndingSoonEventHandler(CurrentWar currentWar);
    public delegate Task WarStartingSoonEventHandler(CurrentWar currentWar);
    public delegate Task ClanVersusPointsChangedEventHandler(Clan clan, int increase);
    public delegate Task ClanPointsChangedEventHandler(Clan clan, int increase);
    public delegate Task WarIsAccessibleChangedEventHandler(CurrentWar currentWar);
    public delegate Task WarEndNotSeenEventHandler(CurrentWar currentWar);
    //public delegate Task VillageChangedEventHandler(Village oldVillage, Village newVillage);
    //public delegate Task VillageDefenseWinsChangedEventHandler(Village village, int increase);
    //public delegate Task VillageExpLevelChangedEventHandler(Village village, int increase);
    //public delegate Task VillageTrophiesChangedEventHandler(Village village, int increase);
    //public delegate Task VillageVersusBattleWinCountChangedEventHandler(Village village, int increase);
    //public delegate Task VillageVersusBattleWinsChangedEventHandler(Village village, int increase);
    //public delegate Task VillageVersusTrophiesChangedEventHandler(Village village, int increase);
    //public delegate Task VillageAchievementsChangedEventHandler(Village village, IReadOnlyList<Achievement> achievements);
    //public delegate Task VillageTroopsChangedEventHandler(Village village, IReadOnlyList<Troop> troops);
    //public delegate Task VillageHeroesChangedEventHandler(Village village, IReadOnlyList<Troop> troops);
    //public delegate Task VillageSpellsChangedEventHandler(Village village, IReadOnlyList<Spell> spells);
    public delegate Task WarStartedEventHandler(CurrentWar currentWar);
    public delegate Task WarEndedEventHandler(CurrentWar currentWar);
    public delegate Task WarEndSeenEventHandler(CurrentWar currentWar);
    public delegate Task LeagueGroupTeamSizeChangedEventHandler(LeagueGroup leagueGroup);
    public delegate Task ClanLabelsChangedEventHandler(Clan clan, IReadOnlyList<ClanLabel> addedLabels, IReadOnlyList<ClanLabel> removedLables);
    //public delegate Task VillageLabelsChangedEventHandler(Village village, IReadOnlyList<VillageLabel> addedLabels, IReadOnlyList<VillageLabel> removedLabels);
    //public delegate Task VillageReachedLegendsLeagueEventHandler(Village village);
    public delegate Task ClanDonationsEventHandler(Clan clan, IReadOnlyList<Donation> receivedDonations, IReadOnlyList<Donation> gaveDonations);
    public delegate Task ClanVillageNameChangedEventHandler(ClanVillage village, string newName);
    public delegate Task ClanVillagesLeagueChangedEventHandler(Clan clan, IReadOnlyList<LeagueChange> leagueChanges);
    public delegate Task ClanVillagesRoleChangedEventHandler(Clan clan, IReadOnlyList<RoleChange> roleChanges);
    public delegate Task MissedAttacksEventHandler(CurrentWar currentWar, IReadOnlyList<Attack> attacks);


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
        ///     <item><description><see cref="CurrentWar.EndTimeUtc"/></description></item>
        ///     <item><description><see cref="CurrentWar.StartTimeUtc"/></description></item>
        ///     <item><description><see cref="CurrentWar.State"/></description></item>
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
        //public event VillageChangedEventHandler? VillageChanged;
        //public event VillageDefenseWinsChangedEventHandler? VillageDefenseWinsChanged;
        //public event VillageExpLevelChangedEventHandler? VillageExpLevelChanged;
        //public event VillageTrophiesChangedEventHandler? VillageTrophiesChanged;
        //public event VillageVersusBattleWinCountChangedEventHandler? VillageVersusBattleWinCountChanged;
        //public event VillageVersusBattleWinsChangedEventHandler? VillageVersusBattleWinsChanged;
        //public event VillageVersusTrophiesChangedEventHandler? VillageVersusTrophiesChanged;
        //public event VillageAchievementsChangedEventHandler? VillageAchievementsChanged;
        //public event VillageTroopsChangedEventHandler? VillageTroopsChanged;
        //public event VillageHeroesChangedEventHandler? VillageHeroesChanged;
        //public event VillageSpellsChangedEventHandler? VillageSpellsChanged;
        public event WarStartedEventHandler? WarStarted;
        /// <summary>
        /// Fires when the <see cref="CurrentWar.EndTimeUtc"/> has elapsed.  The Api may or may not show the war end when this event occurs.
        /// </summary>
        public event WarEndedEventHandler? WarEnded;
        /// <summary>
        /// Fires when the Api shows <see cref="CurrentWar.State"/> is <see cref="Enums.WarState.WarEnded"/>
        /// </summary>
        public event WarEndSeenEventHandler? WarEndSeen;
        /// <summary>
        /// Fires when any clan in a league group has more than 15 attacks.
        /// </summary>
        public event LeagueGroupTeamSizeChangedEventHandler? LeagueGroupTeamSizeChanged;
        public event ClanLabelsChangedEventHandler? ClanLabelsChanged;
        //public event VillageLabelsChangedEventHandler? VillageLabelsChanged;
        //public event VillageReachedLegendsLeagueEventHandler? VillageReachedLegendsLeague;
        public event ClanDonationsEventHandler? ClanDonations;
        public event ClanVillageNameChangedEventHandler? ClanVillageNameChanged;
        public event ClanVillagesLeagueChangedEventHandler? ClanVillagesLeagueChanged;
        public event ClanVillagesRoleChangedEventHandler? ClanVillagesRoleChanged;
        public event MissedAttacksEventHandler? MissedAttacks;

        internal void MissedAttacksEvent(CurrentWar currentWar, List<Attack> missedAttacks)
        {
            if (missedAttacks.Count == 0) return;

             MissedAttacks?.Invoke(currentWar, missedAttacks.ToImmutableArray());
        }

        internal void CrashDetectedEvent()
        {
            try
            {
                Task.Run(async () =>
                {
                    //wait to allow the updater to finish crashing
                    await Task.Delay(5000).ConfigureAwait(false);

                    StartUpdatingClans();
                });
            }
            catch (Exception e)
            {
                _ = Logger?.LogAsync<CocApi>(e, LogLevel.Critical, LoggingEvent.CrashDetected);
            }
        }

        internal void ClanVillagesRoleChangedEvent(Clan clan, List<RoleChange> roleChanges)
        {
            if (roleChanges.Count > 0)
            {
                ClanVillagesRoleChanged?.Invoke(clan, roleChanges.ToImmutableArray());
            }
        }

        internal void ClanVillagesLeagueChangedEvent(Clan clan, List<LeagueChange> leagueChanges)
        {
            if (leagueChanges.Count > 0)
            {
                ClanVillagesLeagueChanged?.Invoke(clan, leagueChanges.ToImmutableArray());
            }
        }

        internal void ClanVillageNameChangedEvent(ClanVillage clanVillage, string oldName) => ClanVillageNameChanged?.Invoke(clanVillage, oldName);

        internal void ClanDonationsEvent(Clan clan, List<Donation> received, List<Donation> donated)
        {
            if (received.Count > 0 || donated.Count > 0)
            {
                ClanDonations?.Invoke(clan, received.ToImmutableArray(), donated.ToImmutableArray());
            }
        }

        //internal void VillageReachedLegendsLeagueEvent(Village village) => VillageReachedLegendsLeague?.Invoke(village);

        internal void ClanLabelsChangedEvent(Clan clan, List<ClanLabel> addedLabels, List<ClanLabel> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

            ClanLabelsChanged?.Invoke(clan, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());
        }

        //internal void VillageLabelsChangedEvent(Village village, List<VillageLabel> addedLabels, List<VillageLabel> removedLabels)
        //{
        //    if (addedLabels.Count == 0 && removedLabels.Count == 0)
        //        return;

        //    VillageLabelsChanged?.Invoke(village, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());
        //}

        internal void LeagueGroupTeamSizeChangedEvent(LeagueGroup leagueGroup) => LeagueGroupTeamSizeChanged?.Invoke(leagueGroup);

        internal void WarEndSeenEvent(CurrentWar currentWar) => WarEndSeen?.Invoke(currentWar);

        internal void WarEndedEvent(CurrentWar currentWar) => WarEnded?.Invoke(currentWar);

        internal void WarStartedEvent(CurrentWar currentWar) => WarStarted?.Invoke(currentWar);

        //internal void VillageSpellsChangedEvent(Village village, List<Spell> spells) => VillageSpellsChanged?.Invoke(village, spells.ToImmutableArray());

        //internal void VillageHeroesChangedEvent(Village village, List<Troop> heroes) => VillageHeroesChanged?.Invoke(village, heroes.ToImmutableArray());

        //internal void VillageTroopsChangedEvent(Village village, List<Troop> troops) => VillageTroopsChanged?.Invoke(village, troops.ToImmutableArray());

        //internal void VillageAchievementsChangedEvent(Village village, List<Achievement> achievements) => VillageAchievementsChanged?.Invoke(village, achievements.ToImmutableArray());

        //internal void VillageVersusTrophiesChangedEvent(Village village, int increase) => VillageVersusTrophiesChanged?.Invoke(village, increase);

        //internal void VillageVersusBattleWinsChangedEvent(Village village, int increase) => VillageVersusBattleWinsChanged?.Invoke(village, increase);

        //internal void VillageVersusBattleWinCountChangedEvent(Village village, int increase) => VillageVersusBattleWinCountChanged?.Invoke(village, increase);

        //internal void VillageTrophiesChangedEvent(Village village, int increase) => VillageTrophiesChanged?.Invoke(village, increase);

        //internal void VillageExpLevelChangedEvent(Village village, int increase) => VillageExpLevelChanged?.Invoke(village, increase);

        //internal void VillageDefenseWinsChangedEvent(Village village, int increase) => VillageDefenseWinsChanged?.Invoke(village, increase);

        //internal void VillageChangedEvent(Village oldVillage, Village newVillage) => VillageChanged?.Invoke(oldVillage, newVillage);

        internal void WarEndNotSeenEvent(CurrentWar currentWar) => WarEndNotSeen?.Invoke(currentWar);

        internal void WarIsAccessibleChangedEvent(CurrentWar currentWar) => WarIsAccessibleChanged?.Invoke(currentWar);

        internal void ClanPointsChangedEvent(Clan clan, int increase) => ClanPointsChanged?.Invoke(clan, increase);

        internal void ClanVersusPointsChangedEvent(Clan clan, int increase) => ClanVersusPointsChanged?.Invoke(clan, increase);

        internal void WarStartingSoonEvent(CurrentWar currentWar) => WarStartingSoon?.Invoke(currentWar);

        internal void WarEndingSoonEvent(CurrentWar currentWar) => WarEndingSoon?.Invoke(currentWar);

        internal void NewAttacksEvent(CurrentWar currentWar, List<Attack> attackApiModels)
        {
            if (attackApiModels.Count > 0)
            {
                NewAttacks?.Invoke(currentWar, attackApiModels.ToImmutableArray());
            }
        }

        internal void WarChangedEvent(CurrentWar oldWar, CurrentWar newWar) => WarChanged?.Invoke(oldWar, newWar);

        public void NewWarEvent(CurrentWar currentWar) => NewWar?.Invoke(currentWar);

        internal void VillagesLeftEvent(Clan clan, List<ClanVillage> clanVillages)
        {
            if (clanVillages.Count > 0)
            {
                VillagesLeft?.Invoke(clan, clanVillages.ToImmutableArray());
            }            
        }

        internal void ClanLocationChangedEvent(Clan oldClan, Clan newClan) => ClanLocationChanged?.Invoke(oldClan, newClan);

        internal void ClanBadgeUrlChangedEvent(Clan oldClan, Clan newClan) => ClanBadgeUrlChanged?.Invoke(oldClan, newClan);

        internal void ClanChangedEvent(Clan oldClan, Clan newClan) => ClanChanged?.Invoke(oldClan, newClan);

        internal void VillagesJoinedEvent(Clan clan, List<ClanVillage> clanVillages)
        {
            if (clanVillages.Count > 0)
            {
                VillagesJoined?.Invoke(clan, clanVillages.ToImmutableArray());
            }
        }
    }
}
