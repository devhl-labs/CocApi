using CocApiLibrary;
using CocApiLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CocApiConsoleTest
{
    public class EventHandlerService
    {
        private readonly LogService _logService;

        private readonly CocApi _cocApi;

        public EventHandlerService(LogService logService, CocApi cocApi)
        {
            _logService = logService;

            _cocApi = cocApi;

            _cocApi.ClanChanged += CocApi_ClanChanged;

            _cocApi.IsAvailableChanged += CocApi_IsAvailableChanged;

            _cocApi.VillagesJoined += CocApi_MembersJoined;

            _cocApi.ClanBadgeUrlChanged += CocApi_ClanBadgeUrlChanged;

            _cocApi.ClanLocationChanged += CocApi_ClanLocationChanged;

            _cocApi.NewAttacks += CocApi_NewAttacks;

            _cocApi.ClanPointsChanged += CocApi_ClanPointsChanged;

            _cocApi.ClanVersusPointsChanged += CocApi_ClanVersusPointsChanged;

            _cocApi.NewWar += CocApi_NewWar;

            _cocApi.WarIsAccessibleChanged += CocApi_WarIsAccessibleChanged;

            _cocApi.LeagueGroupTeamSizeChangeDetected += CocApi_LeagueSizeChangeDetected;

            _cocApi.VillageReachedLegendsLeague += CocApi_VillageReachedLegendsLeague;

            _cocApi.ClanVillageNameChanged += CocApi_ClanVillageNameChanged;

            _cocApi.ClanVillagesLeagueChanged += CocApi_ClanVillagesLeagueChanged; ;

            _cocApi.ClanVillagesRoleChanged += CocApi_ClanVillagesRoleChanged;
        }

        private void CocApi_ClanVillagesLeagueChanged(Dictionary<string, Tuple<ClanVillageAPIModel, VillageLeagueAPIModel>> leagueChanged)
        {
            _logService.LogInformation($"League changed {leagueChanged.First().Key}");
        }

        private void CocApi_ClanVillagesRoleChanged(Dictionary<string, Tuple<ClanVillageAPIModel, Enums.Role>> roleChanges)
        {
            _logService.LogInformation($"New role: {roleChanges.First().Value.Item2}");
        }

        private void CocApi_ClanVillageNameChanged(ClanVillageAPIModel oldMember, string newName)
        {
            _logService.LogInformation($"New name: {newName}");
        }

        private void CocApi_VillageReachedLegendsLeague(VillageAPIModel villageAPIModel)
        {
            _logService.LogInformation($"Village reached legends: {villageAPIModel.Name}");
        }

        private void CocApi_WarIsAccessibleChanged(ICurrentWarAPIModel currentWarAPIModel)
        {
            _logService.LogInformation($"War is accessible changed:{currentWarAPIModel.Flags.WarIsAccessible}");
        }

        private void CocApi_NewWar(ICurrentWarAPIModel currentWarAPIModel)
        {
            _logService.LogInformation($"New War: {currentWarAPIModel.WarId}");
        }

        private void CocApi_ClanVersusPointsChanged(ClanAPIModel oldClan, int newClanVersusPoints)
        {
            _logService.LogInformation($"{oldClan.ClanTag} {oldClan.Name} new clan versus points: {newClanVersusPoints}");
        }

        private void CocApi_ClanPointsChanged(ClanAPIModel oldClan, int newClanPoints)
        {
            _logService.LogInformation($"{oldClan.ClanTag} {oldClan.Name} new clan points: {newClanPoints}");
        }

        private void CocApi_ClanLocationChanged(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            _logService.LogInformation(newClan.Location?.Name);
        }

        private void CocApi_ClanBadgeUrlChanged(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            _logService.LogInformation(newClan.BadgeUrls?.Large);
        }

        private void CocApi_ClanChanged(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            _logService.LogInformation($"{oldClan.ClanTag} {oldClan.Name} changed.");
        }

        private void CocApi_NewAttacks(ICurrentWarAPIModel currentWarAPIModel, List<AttackAPIModel> attackAPIModels)
        {
            _logService.LogInformation($"new attacks: {attackAPIModels.Count()}");
        }

        private void CocApi_MembersJoined(ClanAPIModel clanAPIModel, List<ClanVillageAPIModel> memberListAPIModels)
        {
            _logService.LogInformation($"{memberListAPIModels.Count()} members joined.");
        }

        private void CocApi_IsAvailableChanged(bool isAvailable)
        {
            _logService.LogInformation($"CocApi isAvailable: {isAvailable}");
        }

        private void CocApi_LeagueSizeChangeDetected(LeagueGroupAPIModel leagueGroupAPIModel)
        {
            _logService.LogInformation($"League Size changed: {leagueGroupAPIModel.TeamSize}");
        }



    }
}
