using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;

using System.Collections.Immutable;

namespace devhl.CocApi
{
    public delegate Task NewWarEventHandler(CurrentWar currentWar);
    public delegate Task WarChangedEventHandler(CurrentWar oldWar, CurrentWar newWar);
    public delegate Task NewAttacksEventHandler(CurrentWar currentWar, IReadOnlyList<Attack> newAttacks);
    public delegate Task WarEndingSoonEventHandler(CurrentWar currentWar);
    public delegate Task WarStartingSoonEventHandler(CurrentWar currentWar);
    public delegate Task WarIsAccessibleChangedEventHandler(CurrentWar currentWar);
    public delegate Task WarEndNotSeenEventHandler(CurrentWar currentWar);
    public delegate Task WarStartedEventHandler(CurrentWar currentWar);
    public delegate Task WarEndedEventHandler(CurrentWar currentWar);
    public delegate Task WarEndSeenEventHandler(CurrentWar currentWar);
    public delegate Task LeagueGroupTeamSizeChangedEventHandler(LeagueGroup leagueGroup);
    public delegate Task MissedAttacksEventHandler(CurrentWar currentWar, IReadOnlyList<Attack> attacks);
    public delegate Task InitialDownloadEventHandler(IReadOnlyList<CurrentWar> currentWars);


    public sealed partial class CocApi : IDisposable
    {
        ///// <summary>
        ///// Fires if you query the Api during an outage.
        ///// If the service is not available, you may still try to query the Api if you wish.
        ///// </summary>
        //public event ApiIsAvailableChangedEventHandler? ApiIsAvailableChanged;
        ///// <summary>
        ///// Fires if the following properties change:
        ///// <list type="bullet">
        /////    <item><description><see cref="Clan.ClanLevel"/></description></item>
        /////    <item><description><see cref="Clan.Description"/></description></item>
        /////    <item><description><see cref="Clan.IsWarLogPublic"/></description></item>
        /////    <item><description><see cref="Clan.Name"/></description></item>
        /////    <item><description><see cref="Clan.RequiredTrophies"/>RequiredTrophies</description></item>
        /////    <item><description><see cref="Clan.Recruitment"/></description></item>
        /////    <item><description><see cref="Clan.VillageCount"/></description></item>
        /////    <item><description><see cref="Clan.WarLosses"/></description></item>
        /////    <item><description><see cref="Clan.WarWins"/></description></item>
        /////    <item><description><see cref="Clan.Wars"/></description></item>
        /////    <item><description><see cref="Clan.WarTies"/></description></item>
        /////    <item><description><see cref="Clan.WarFrequency"/></description></item>
        ///// </list>
        ///// </summary>
        //public event ClanChangedEventHandler? ClanChanged;
        //public event VillagesJoinedEventHandler? VillagesJoined;
        //public event VillagesLeftEventHandler? VillagesLeft;
        //public event ClanBadgeUrlChangedEventHandler? ClanBadgeUrlChanged;
        //public event ClanLocationChangedEventHandler? ClanLocationChanged;
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
        public event MissedAttacksEventHandler? MissedAttacks;
        public event InitialDownloadEventHandler? InitialDownload;

        internal void MissedAttacksEvent(CurrentWar currentWar, List<Attack> missedAttacks)
        {
            if (missedAttacks.Count == 0) return;

             MissedAttacks?.Invoke(currentWar, missedAttacks.ToImmutableArray());
        }

        internal void LeagueGroupTeamSizeChangedEvent(LeagueGroup leagueGroup) => LeagueGroupTeamSizeChanged?.Invoke(leagueGroup);

        internal void WarEndSeenEvent(CurrentWar currentWar) => WarEndSeen?.Invoke(currentWar);

        internal void WarEndedEvent(CurrentWar currentWar) => WarEnded?.Invoke(currentWar);

        internal void WarStartedEvent(CurrentWar currentWar) => WarStarted?.Invoke(currentWar);

        internal void WarEndNotSeenEvent(CurrentWar currentWar) => WarEndNotSeen?.Invoke(currentWar);

        internal void WarIsAccessibleChangedEvent(CurrentWar currentWar) => WarIsAccessibleChanged?.Invoke(currentWar);

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

        public void InitialDownloadEvent(List<CurrentWar> currentWars)
        {
            if (currentWars.Count > 0)
            {
                InitialDownload?.Invoke(currentWars.ToImmutableArray());
            }
        }
    }
}
