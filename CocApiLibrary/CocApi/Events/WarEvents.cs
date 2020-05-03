//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using devhl.CocApi.Models.Clan;
//using devhl.CocApi.Models.Village;
//using devhl.CocApi.Models.War;

//using System.Collections.Immutable;

//namespace devhl.CocApi
//{
    //public delegate Task NewWarEventHandler(CurrentWar currentWar);
    //public delegate Task WarChangedEventHandler(CurrentWar oldWar, CurrentWar newWar);
    //public delegate Task NewAttacksEventHandler(CurrentWar currentWar, IReadOnlyList<Attack> newAttacks);
    //public delegate Task WarEndingSoonEventHandler(CurrentWar currentWar);
    //public delegate Task WarStartingSoonEventHandler(CurrentWar currentWar);
    //public delegate Task WarIsAccessibleChangedEventHandler(CurrentWar currentWar, bool canRead);
    //public delegate Task WarEndNotSeenEventHandler(CurrentWar currentWar);
    //public delegate Task WarStartedEventHandler(CurrentWar currentWar);
    //public delegate Task WarEndedEventHandler(CurrentWar currentWar);
    //public delegate Task WarEndSeenEventHandler(CurrentWar currentWar);
    //public delegate Task LeagueGroupTeamSizeChangedEventHandler(LeagueGroup leagueGroup);


    //public sealed partial class CocApi : IDisposable
    //{
        //public event NewWarEventHandler? NewWar;
        ///// <summary>
        ///// Fires if the following properties change:
        ///// <list type="bullet">
        /////     <item><description><see cref="CurrentWar.EndTimeUtc"/></description></item>
        /////     <item><description><see cref="CurrentWar.StartTimeUtc"/></description></item>
        /////     <item><description><see cref="CurrentWar.State"/></description></item>
        ///// 
        ///// </list>
        ///// </summary>
        //public event WarChangedEventHandler? WarChanged;
        //public event NewAttacksEventHandler? NewAttacks;
        //public event WarEndingSoonEventHandler? WarEndingSoon;
        //public event WarStartingSoonEventHandler? WarStartingSoon;
        ///// <summary>
        ///// Fires if the war cannot be found from either clanTags or warTag.  Private war logs can also fire this.
        ///// </summary>
        //public event WarIsAccessibleChangedEventHandler? WarIsAccessibleChanged;
        ///// <summary>
        ///// Fires when the war is not accessible and the end time has passed.  
        ///// This war may still become available if one of the clans does not spin and opens their war log.
        ///// </summary>
        //public event WarEndNotSeenEventHandler? WarEndNotSeen;
        ///// <summary>
        ///// Fires if the following properties change:
        ///// <list type="bullet">
        /////    <item><description><see cref="Village.AttackWins"/></description></item>
        /////    <item><description><see cref="Village.BestTrophies"/></description></item>
        /////    <item><description><see cref="Village.BestVersusTrophies"/></description></item>
        /////    <item><description><see cref="Village.BuilderHallLevel"/></description></item>
        /////    <item><description><see cref="Village.TownHallLevel"/></description></item>
        /////    <item><description><see cref="Village.TownHallWeaponLevel"/></description></item>
        /////    <item><description><see cref="Village.WarStars"/></description></item>
        ///// </list>
        ///// </summary>
        //public event WarStartedEventHandler? WarStarted;
        ///// <summary>
        ///// Fires when the <see cref="CurrentWar.EndTimeUtc"/> has elapsed.  The Api may or may not show the war end when this event occurs.
        ///// </summary>
        //public event WarEndedEventHandler? WarEnded;
        ///// <summary>
        ///// Fires when the Api shows <see cref="CurrentWar.State"/> is <see cref="Enums.WarState.WarEnded"/>
        ///// </summary>
        //public event WarEndSeenEventHandler? WarEndSeen;
        ///// <summary>
        ///// Fires when any clan in a league group has more than 15 attacks.
        ///// </summary>
        //public event LeagueGroupTeamSizeChangedEventHandler? LeagueGroupTeamSizeChanged;

        //internal void LeagueGroupTeamSizeChangedEvent(LeagueGroup leagueGroup) => LeagueGroupTeamSizeChanged?.Invoke(leagueGroup);

        //internal void WarEndSeenEvent(CurrentWar currentWar) => WarEndSeen?.Invoke(currentWar);

        //internal void WarEndedEvent(CurrentWar currentWar) => WarEnded?.Invoke(currentWar);

        //internal void WarStartedEvent(CurrentWar currentWar) => WarStarted?.Invoke(currentWar);

        //internal void WarEndNotSeenEvent(CurrentWar currentWar) => WarEndNotSeen?.Invoke(currentWar);

        //internal void WarIsAccessibleChangedEvent(CurrentWar currentWar, bool canRead) => WarIsAccessibleChanged?.Invoke(currentWar, canRead);

        //internal void WarStartingSoonEvent(CurrentWar currentWar) => WarStartingSoon?.Invoke(currentWar);

        //internal void WarEndingSoonEvent(CurrentWar currentWar) => WarEndingSoon?.Invoke(currentWar);

        //internal void NewAttacksEvent(CurrentWar currentWar, List<Attack> attackApiModels)
        //{
        //    if (attackApiModels.Count > 0)
        //    {
        //        NewAttacks?.Invoke(currentWar, attackApiModels.ToImmutableArray());
        //    }
        //}

        //internal void WarChangedEvent(CurrentWar oldWar, CurrentWar newWar) => WarChanged?.Invoke(oldWar, newWar);

        //internal void NewWarEvent(CurrentWar currentWar) => NewWar?.Invoke(currentWar);
//    }
//}
