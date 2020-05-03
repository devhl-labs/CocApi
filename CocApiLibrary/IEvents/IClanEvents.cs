//using devhl.CocApi.Models;
//using devhl.CocApi.Models.Clan;
//using devhl.CocApi.Models.Village;
//using devhl.CocApi.Models.War;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace devhl.CocApi
//{
//    public interface IClanEvents
//    {
//        Task ClanBadgeUrlChangedAsync(Clan oldClan, Clan newClan);
//        Task ClanChangedAsync(Clan oldClan, Clan newClan);
//        Task ClanDonationsAsync(Clan oldClan, IReadOnlyList<Donation> receivedDonations, IReadOnlyList<Donation> gaveDonations);
//        Task ClanLabelsChangedAsync(Clan clan, IReadOnlyList<ClanLabel> addedLabels, IReadOnlyList<ClanLabel> removedLabels);
//        Task ClanLocationChangedAsync(Clan oldClan, Clan newClan);
//        Task ClanPointsChangedAsync(Clan oldClan, int increase);
//        Task ClanVersusPointsChangedAsync(Clan clan, int increase);
//        Task ClanVillageNameChangedAsync(ClanVillage clanVillage, string oldName);
//        Task ClanVillagesLeagueChangedAsync(Clan clan, IReadOnlyList<LeagueChange> leagueChanges);
//        Task ClanVillagesRoleChangedAsync(Clan clan, IReadOnlyList<RoleChange> roleChanges);
//        Task ClanVillagesJoinedAsync(Clan clan, IReadOnlyList<ClanVillage> clanVillages);
//        Task ClanVillagesLeftAsync(Clan clan, IReadOnlyList<ClanVillage> clanVillages);
//        Task WarLeagueChangedAsync(Clan clan, WarLeague? oldWarLeague);        
//    }
//}
