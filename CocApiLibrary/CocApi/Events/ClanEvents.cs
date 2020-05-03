//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using devhl.CocApi.Models.Clan;
//using devhl.CocApi.Models.Village;
//using devhl.CocApi.Models.War;

//using System.Collections.Immutable;
//using devhl.CocApi.Models;

//namespace devhl.CocApi
//{
//    public delegate Task ClanChangedEventHandler(Clan oldClan, Clan newClan);
//    public delegate Task ClanVillagesJoinedEventHandler(Clan clan, IReadOnlyList<ClanVillage> clanVillages);
//    public delegate Task ClanVillagesLeftEventHandler(Clan clan, IReadOnlyList<ClanVillage> clanVillages);
//    public delegate Task ClanBadgeUrlChangedEventHandler(Clan oldClan, Clan newClan);
//    public delegate Task ClanLocationChangedEventHandler(Clan oldClan, Clan newClan);
//    public delegate Task ClanVersusPointsChangedEventHandler(Clan clan, int increase);
//    public delegate Task ClanPointsChangedEventHandler(Clan clan, int increase);
//    public delegate Task ClanLabelsChangedEventHandler(Clan clan, IReadOnlyList<ClanLabel> addedLabels, IReadOnlyList<ClanLabel> removedLables);
//    public delegate Task ClanDonationsEventHandler(Clan clan, IReadOnlyList<Donation> receivedDonations, IReadOnlyList<Donation> gaveDonations);
//    public delegate Task ClanVillageNameChangedEventHandler(ClanVillage village, string newName);
//    public delegate Task ClanVillagesLeagueChangedEventHandler(Clan clan, IReadOnlyList<LeagueChange> leagueChanges);
//    public delegate Task ClanVillagesRoleChangedEventHandler(Clan clan, IReadOnlyList<RoleChange> roleChanges);
//    public delegate Task WarLeagueChangedEventHandler(Clan clan, WarLeague? oldWarLeague);

//    public sealed partial class CocApi : IDisposable
//    {
//        /// <summary>
//        /// Fires if the following properties change:
//        /// <list type="bullet">
//        ///    <item><description><see cref="Clan.ClanLevel"/></description></item>
//        ///    <item><description><see cref="Clan.Description"/></description></item>
//        ///    <item><description><see cref="Clan.IsWarLogPublic"/></description></item>
//        ///    <item><description><see cref="Clan.Name"/></description></item>
//        ///    <item><description><see cref="Clan.RequiredTrophies"/>RequiredTrophies</description></item>
//        ///    <item><description><see cref="Clan.Recruitment"/></description></item>
//        ///    <item><description><see cref="Clan.VillageCount"/></description></item>
//        ///    <item><description><see cref="Clan.WarLosses"/></description></item>
//        ///    <item><description><see cref="Clan.WarWins"/></description></item>
//        ///    <item><description><see cref="Clan.Wars"/></description></item>
//        ///    <item><description><see cref="Clan.WarTies"/></description></item>
//        ///    <item><description><see cref="Clan.WarFrequency"/></description></item>
//        /// </list>
//        /// </summary>
//        public event ClanChangedEventHandler? ClanChanged;
//        public event ClanVillagesJoinedEventHandler? ClanVillagesJoined;
//        public event ClanVillagesLeftEventHandler? ClanVillagesLeft;
//        public event ClanBadgeUrlChangedEventHandler? ClanBadgeUrlChanged;
//        public event ClanLocationChangedEventHandler? ClanLocationChanged;
//        public event ClanVersusPointsChangedEventHandler? ClanVersusPointsChanged;
//        public event ClanPointsChangedEventHandler? ClanPointsChanged;
//        public event ClanLabelsChangedEventHandler? ClanLabelsChanged;
//        public event ClanDonationsEventHandler? ClanDonations;
//        public event ClanVillageNameChangedEventHandler? ClanVillageNameChanged;
//        public event ClanVillagesLeagueChangedEventHandler? ClanVillagesLeagueChanged;
//        public event ClanVillagesRoleChangedEventHandler? ClanVillagesRoleChanged;
//        public event WarLeagueChangedEventHandler? WarLeagueChanged;


//        internal void WarLeagueChangedEvent(Clan clan, WarLeague? oldWarLeague)
//        {
//            WarLeagueChanged?.Invoke(clan, oldWarLeague);
//        }

//        internal void ClanVillagesRoleChangedEvent(Clan clan, List<RoleChange> roleChanges)
//        {
//            if (roleChanges.Count > 0)
//            {
//                ClanVillagesRoleChanged?.Invoke(clan, roleChanges.ToImmutableArray());
//            }
//        }

//        internal void ClanVillagesLeagueChangedEvent(Clan clan, List<LeagueChange> leagueChanges)
//        {
//            if (leagueChanges.Count > 0)
//            {
//                ClanVillagesLeagueChanged?.Invoke(clan, leagueChanges.ToImmutableArray());
//            }
//        }

//        internal void ClanVillageNameChangedEvent(ClanVillage clanVillage, string oldName) => ClanVillageNameChanged?.Invoke(clanVillage, oldName);

//        internal void ClanDonationsEvent(Clan clan, List<Donation> received, List<Donation> donated)
//        {
//            if (received.Count > 0 || donated.Count > 0)
//            {
//                ClanDonations?.Invoke(clan, received.ToImmutableArray(), donated.ToImmutableArray());
//            }
//        }

//        internal void ClanLabelsChangedEvent(Clan clan, List<ClanLabel> addedLabels, List<ClanLabel> removedLabels)
//        {
//            if (addedLabels.Count == 0 && removedLabels.Count == 0) return;

//            ClanLabelsChanged?.Invoke(clan, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray());
//        }

//        internal void ClanPointsChangedEvent(Clan clan, int increase) => ClanPointsChanged?.Invoke(clan, increase);

//        internal void ClanVersusPointsChangedEvent(Clan clan, int increase) => ClanVersusPointsChanged?.Invoke(clan, increase);

//        internal void ClanVillagesLeftEvent(Clan clan, List<ClanVillage> clanVillages)
//        {
//            if (clanVillages.Count > 0)
//            {
//                ClanVillagesLeft?.Invoke(clan, clanVillages.ToImmutableArray());
//            }            
//        }

//        internal void ClanLocationChangedEvent(Clan oldClan, Clan newClan) => ClanLocationChanged?.Invoke(oldClan, newClan);

//        internal void ClanBadgeUrlChangedEvent(Clan oldClan, Clan newClan) => ClanBadgeUrlChanged?.Invoke(oldClan, newClan);

//        internal void ClanChangedEvent(Clan oldClan, Clan newClan) => ClanChanged?.Invoke(oldClan, newClan);

//        internal void ClanVillagesJoinedEvent(Clan clan, List<ClanVillage> clanVillages)
//        {
//            if (clanVillages.Count > 0)
//            {
//                ClanVillagesJoined?.Invoke(clan, clanVillages.ToImmutableArray());
//            }
//        }
//    }
//}
