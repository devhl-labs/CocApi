using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

using devhl.CocApi;
using devhl.CocApi.Models;
using System.Threading.Tasks;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.War;
using devhl.CocApi.Models.Village;
using System.Collections.Immutable;

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

            _cocApi.ClanChanged += ClanChanged;

            _cocApi.ApiIsAvailableChanged += IsAvailableChanged;

            _cocApi.VillagesJoined += MembersJoined;

            _cocApi.ClanBadgeUrlChanged += ClanBadgeUrlChanged;

            _cocApi.ClanLocationChanged += ClanLocationChanged;

            _cocApi.NewAttacks += NewAttacks;

            _cocApi.ClanPointsChanged += ClanPointsChanged;

            _cocApi.ClanVersusPointsChanged += ClanVersusPointsChanged;

            _cocApi.NewWar += NewWar;

            _cocApi.WarIsAccessibleChanged += WarIsAccessibleChanged;

            _cocApi.LeagueGroupTeamSizeChanged += LeagueSizeChangeDetected;

            _cocApi.VillageReachedLegendsLeague += VillageReachedLegendsLeague;

            _cocApi.ClanVillageNameChanged += ClanVillageNameChanged;

            _cocApi.ClanVillagesLeagueChanged += ClanVillagesLeagueChanged;

            _cocApi.ClanVillagesRoleChanged += ClanVillagesRoleChanged;

            _cocApi.WarStarted += WarStarted;

            _cocApi.WarStartingSoon += WarStartingSoon;
        }

        public Task CrashDetected(Exception e)
        {
            _logService.LogInformation($"Crash detected on updater: {e.Message}");

            _cocApi.StartUpdatingClans();

            return Task.CompletedTask;
        }

        public Task WarStartingSoon(IActiveWar currentWarApiModel)
        {
            _logService.LogInformation("war starting soon");

            return Task.CompletedTask;
        }

        public Task WarStarted(IActiveWar currentWarApiModel)
        {
            _logService.LogInformation("war started");

            return Task.CompletedTask;
        }

        public Task ClanVillagesLeagueChanged(Clan oldClan, IReadOnlyList<LeagueChange> leagueChanged)
        {
            _logService.LogInformation($"League changed {leagueChanged.First().Village.Name}");

            return Task.CompletedTask;
        }

        public Task ClanVillagesRoleChanged(Clan clan, IReadOnlyList<RoleChange> roleChanges)
        {
            _logService.LogInformation($"New role: {roleChanges.First().Village.Name}");

            return Task.CompletedTask;
        }

        public Task ClanVillageNameChanged(ClanVillage oldMember, string newName)
        {
            _logService.LogInformation($"New name: {newName}");

            return Task.CompletedTask;
        }

        public Task VillageReachedLegendsLeague(Village villageApiModel)
        {
            _logService.LogInformation($"Village reached legends: {villageApiModel.Name}");

            return Task.CompletedTask;
        }

        public Task WarIsAccessibleChanged(IActiveWar currentWarApiModel)
        {
            _logService.LogInformation($"War is accessible changed:{currentWarApiModel.Flags.WarIsAccessible}");

            return Task.CompletedTask;
        }

        public Task NewWar(IActiveWar currentWarApiModel)
        {
            _logService.LogInformation($"New War: {currentWarApiModel.WarId}");

            return Task.CompletedTask;
        }

        public Task ClanVersusPointsChanged(Clan oldClan, int newClanVersusPoints)
        {
            _logService.LogInformation($"{oldClan.ClanTag} {oldClan.Name} new clan versus points: {newClanVersusPoints}");

            return Task.CompletedTask;
        }

        public Task ClanPointsChanged(Clan oldClan, int newClanPoints)
        {
            _logService.LogInformation($"{oldClan.ClanTag} {oldClan.Name} new clan points: {newClanPoints}");

            return Task.CompletedTask;
        }

        public Task ClanLocationChanged(Clan oldClan, Clan newClan)
        {
            _logService.LogInformation(newClan.Location?.Name);

            return Task.CompletedTask;
        }

        public Task ClanBadgeUrlChanged(Clan oldClan, Clan newClan)
        {
            _logService.LogInformation(newClan.BadgeUrl?.Large);

            return Task.CompletedTask;
        }

        public Task ClanChanged(Clan oldClan, Clan newClan)
        {
            _logService.LogInformation($"{oldClan.ClanTag} {oldClan.Name} changed.");

            return Task.CompletedTask;
        }

        public Task NewAttacks(IActiveWar currentWarApiModel, IReadOnlyList<Attack> attackApiModels)
        {
            _logService.LogInformation($"new attacks: {attackApiModels.Count()}");

            return Task.CompletedTask;
        }

        public Task MembersJoined(Clan clanApiModel, IReadOnlyList<ClanVillage> memberListApiModels)
        {
            _logService.LogInformation($"{memberListApiModels.Count()} members joined.");

            return Task.CompletedTask;
        }

        public Task IsAvailableChanged(bool isAvailable)
        {
            _logService.LogInformation($"CocApi isAvailable: {isAvailable}");

            return Task.CompletedTask;
        }

        public Task LeagueSizeChangeDetected(LeagueGroup leagueGroupApiModel)
        {
            _logService.LogInformation($"League Size changed: {leagueGroupApiModel.TeamSize}");

            return Task.CompletedTask;
        }



    }
}
