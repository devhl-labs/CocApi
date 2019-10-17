using CocApiLibrary;
using CocApiLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CocApiConsoleTest
{
    public class EventHandler
    {
        private readonly LogService _logService;

        private readonly CocApi _cocApi;

        public EventHandler(LogService logService, CocApi cocApi)
        {
            _logService = logService;

            _cocApi = cocApi;

            _cocApi.ClanChanged += CocApi_ClanChanged;

            _cocApi.IsAvailableChanged += CocApi_IsAvailableChanged;

            _cocApi.MembersJoined += CocApi_MembersJoined;

            _cocApi.ClanBadgeUrlChanged += CocApi_ClanBadgeUrlChanged;

            _cocApi.ClanLocationChanged += CocApi_ClanLocationChanged;

            _cocApi.NewAttacks += CocApi_NewAttacks;

            _cocApi.ClanPointsChanged += CocApi_ClanPointsChanged;

            _cocApi.ClanVersusPointsChanged += CocApi_ClanVersusPointsChanged;

            _cocApi.NewWar += CocApi_NewWar;

            _cocApi.WarIsAccessibleChanged += CocApi_WarIsAccessibleChanged;

            _cocApi.LeagueGroupTeamSizeChangeDetected += CocApi_LeagueSizeChangeDetected;

            _cocApi.VillageReachedLegendsLeague += CocApi_VillageReachedLegendsLeague;

            _cocApi.ClanMemberNameChanged += CocApi_ClanMemberNameChanged;

            _cocApi.ClanMembersLeagueChanged += CocApi_ClanMembersLeagueChanged;

            _cocApi.ClanMembersRoleChanged += CocApi_ClanMembersRoleChanged;
        }

        private void CocApi_ClanMembersRoleChanged(Dictionary<string, Tuple<MemberListAPIModel, Enums.Role>> roleChanges)
        {
            Console.WriteLine($"New role: {roleChanges.First().Value.Item2}");
        }

        private void CocApi_ClanMembersLeagueChanged(Dictionary<string, Tuple<MemberListAPIModel, MemberListAPIModel>> leagueChanged)
        {
            Console.WriteLine($"League changed {leagueChanged.First().Key}");
        }

        private void CocApi_ClanMemberNameChanged(MemberListAPIModel oldMember, string newName)
        {
            Console.WriteLine($"New name: {newName}");
        }

        private void CocApi_VillageReachedLegendsLeague(VillageAPIModel villageAPIModel)
        {
            Console.WriteLine($"Village reached legends: {villageAPIModel.Name}");
        }

        private void CocApi_WarIsAccessibleChanged(ICurrentWarAPIModel currentWarAPIModel)
        {
            Console.WriteLine($"War is accessible changed:{currentWarAPIModel.Flags.WarIsAccessible}");
        }

        private void CocApi_NewWar(ICurrentWarAPIModel currentWarAPIModel)
        {
            Console.WriteLine($"New War: {currentWarAPIModel.WarID}");
        }

        private void CocApi_ClanVersusPointsChanged(ClanAPIModel oldClan, int newClanVersusPoints)
        {
            Console.WriteLine($"{oldClan.Tag} {oldClan.Name} new clan versus points: {newClanVersusPoints}");
        }

        private void CocApi_ClanPointsChanged(ClanAPIModel oldClan, int newClanPoints)
        {
            Console.WriteLine($"{oldClan.Tag} {oldClan.Name} new clan points: {newClanPoints}");
        }

        private void CocApi_ClanLocationChanged(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            Console.WriteLine(newClan.Location?.Name);
        }

        private void CocApi_ClanBadgeUrlChanged(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            Console.WriteLine(newClan.BadgeUrls?.Large);
        }

        private void CocApi_ClanChanged(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            Console.WriteLine($"{oldClan.Tag} {oldClan.Name} changed.");
        }

        private void CocApi_NewAttacks(ICurrentWarAPIModel currentWarAPIModel, List<AttackAPIModel> attackAPIModels)
        {
            Console.WriteLine($"new attacks: {attackAPIModels.Count()}");
        }

        private void CocApi_MembersJoined(ClanAPIModel clanAPIModel, List<MemberListAPIModel> memberListAPIModels)
        {
            Console.WriteLine($"{memberListAPIModels.Count()} members joined.");
        }

        private void CocApi_IsAvailableChanged(bool isAvailable)
        {
            Console.WriteLine($"CocApi isAvailable: {isAvailable}");
        }

        private void CocApi_LeagueSizeChangeDetected(LeagueGroupAPIModel leagueGroupAPIModel)
        {
            Console.WriteLine($"League Size changed: {leagueGroupAPIModel.TeamSize}");
        }



    }
}
