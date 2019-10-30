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

        private void CocApi_ClanVillagesLeagueChanged(Dictionary<string, Tuple<ClanVillageApiModel, VillageLeagueApiModel>> leagueChanged)
        {
            _logService.LogInformation($"League changed {leagueChanged.First().Key}");
        }

        private void CocApi_ClanVillagesRoleChanged(Dictionary<string, Tuple<ClanVillageApiModel, Enums.Role>> roleChanges)
        {
            _logService.LogInformation($"New role: {roleChanges.First().Value.Item2}");
        }

        private void CocApi_ClanVillageNameChanged(ClanVillageApiModel oldMember, string newName)
        {
            _logService.LogInformation($"New name: {newName}");
        }

        private void CocApi_VillageReachedLegendsLeague(VillageApiModel villageApiModel)
        {
            _logService.LogInformation($"Village reached legends: {villageApiModel.Name}");
        }

        private void CocApi_WarIsAccessibleChanged(ICurrentWarApiModel currentWarApiModel)
        {
            _logService.LogInformation($"War is accessible changed:{currentWarApiModel.Flags.WarIsAccessible}");
        }

        private void CocApi_NewWar(ICurrentWarApiModel currentWarApiModel)
        {
            _logService.LogInformation($"New War: {currentWarApiModel.WarId}");
        }

        private void CocApi_ClanVersusPointsChanged(ClanApiModel oldClan, int newClanVersusPoints)
        {
            _logService.LogInformation($"{oldClan.ClanTag} {oldClan.Name} new clan versus points: {newClanVersusPoints}");
        }

        private void CocApi_ClanPointsChanged(ClanApiModel oldClan, int newClanPoints)
        {
            _logService.LogInformation($"{oldClan.ClanTag} {oldClan.Name} new clan points: {newClanPoints}");
        }

        private void CocApi_ClanLocationChanged(ClanApiModel oldClan, ClanApiModel newClan)
        {
            _logService.LogInformation(newClan.Location?.Name);
        }

        private void CocApi_ClanBadgeUrlChanged(ClanApiModel oldClan, ClanApiModel newClan)
        {
            _logService.LogInformation(newClan.BadgeUrls?.Large);
        }

        private void CocApi_ClanChanged(ClanApiModel oldClan, ClanApiModel newClan)
        {
            _logService.LogInformation($"{oldClan.ClanTag} {oldClan.Name} changed.");
        }

        private void CocApi_NewAttacks(ICurrentWarApiModel currentWarApiModel, List<AttackApiModel> attackApiModels)
        {
            _logService.LogInformation($"new attacks: {attackApiModels.Count()}");
        }

        private void CocApi_MembersJoined(ClanApiModel clanApiModel, List<ClanVillageApiModel> memberListApiModels)
        {
            _logService.LogInformation($"{memberListApiModels.Count()} members joined.");
        }

        private void CocApi_IsAvailableChanged(bool isAvailable)
        {
            _logService.LogInformation($"CocApi isAvailable: {isAvailable}");
        }

        private void CocApi_LeagueSizeChangeDetected(LeagueGroupApiModel leagueGroupApiModel)
        {
            _logService.LogInformation($"League Size changed: {leagueGroupApiModel.TeamSize}");
        }



    }
}
