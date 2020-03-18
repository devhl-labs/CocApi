using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly ILogger _logger;

        private readonly CocApi _cocApi;

        public EventHandlerService(ILogger logger, CocApi cocApi)
        {
            _logger = logger;

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

        public Task WarStartingSoon(CurrentWar currentWar)
        {
            _logger.LogAsync<EventHandlerService>("War starting soon");

            return Task.CompletedTask;
        }

        public Task WarStarted(CurrentWar currentWar)
        {
            _logger.LogAsync<EventHandlerService>("War started");

            return Task.CompletedTask;
        }

        public Task ClanVillagesLeagueChanged(Clan oldClan, IReadOnlyList<LeagueChange> leagueChanged)
        {
            _logger.LogAsync<EventHandlerService>($"League changed {leagueChanged.First().Village.Name}");

            return Task.CompletedTask;
        }

        public Task ClanVillagesRoleChanged(Clan clan, IReadOnlyList<RoleChange> roleChanges)
        {
            _logger.LogAsync<EventHandlerService>($"New role: {roleChanges.First().Village.Name}");

            return Task.CompletedTask;
        }

        public Task ClanVillageNameChanged(ClanVillage oldMember, string newName)
        {
            _logger.LogAsync<EventHandler>($"New name: {newName}");

            return Task.CompletedTask;
        }

        public Task VillageReachedLegendsLeague(Village villageApiModel)
        {
            _logger.LogAsync<EventHandlerService>($"Village reached legends: {villageApiModel.Name}");

            return Task.CompletedTask;
        }

        public Task WarIsAccessibleChanged(CurrentWar currentWar)
        {
            _logger.LogAsync<EventHandlerService>($"War is accessible changed:{currentWar.Flags.WarIsAccessible}");

            return Task.CompletedTask;
        }

        public Task NewWar(CurrentWar currentWar)
        {
            _logger.LogAsync<EventHandlerService>($"New War: {currentWar.WarKey}");

            return Task.CompletedTask;
        }

        public Task ClanVersusPointsChanged(Clan oldClan, int newClanVersusPoints)
        {
            _logger.LogAsync<EventHandlerService>($"{oldClan.ClanTag} {oldClan.Name} new clan versus points: {newClanVersusPoints}");

            return Task.CompletedTask;
        }

        public Task ClanPointsChanged(Clan oldClan, int newClanPoints)
        {
            _logger.LogAsync<EventHandlerService>($"{oldClan.ClanTag} {oldClan.Name} new clan points: {newClanPoints}");

            return Task.CompletedTask;
        }

        public Task ClanLocationChanged(Clan oldClan, Clan newClan)
        {
            _logger.LogAsync<EventHandlerService>(newClan.Location?.Name);

            return Task.CompletedTask;
        }

        public Task ClanBadgeUrlChanged(Clan oldClan, Clan newClan)
        {
            _logger.LogAsync<EventHandlerService>(newClan.BadgeUrl?.Large);

            return Task.CompletedTask;
        }

        public Task ClanChanged(Clan oldClan, Clan newClan)
        {
            _logger.LogAsync<EventHandlerService>($"{oldClan.ClanTag} {oldClan.Name} changed.");

            return Task.CompletedTask;
        }

        public Task NewAttacks(CurrentWar currentWar, IReadOnlyList<Attack> attackApiModels)
        {
            _logger.LogAsync<EventHandlerService>($"new attacks: {attackApiModels.Count()}");

            return Task.CompletedTask;
        }

        public Task MembersJoined(Clan clanApiModel, IReadOnlyList<ClanVillage> memberListApiModels)
        {
            _logger.LogAsync<EventHandlerService>($"{memberListApiModels.Count()} members joined.");

            return Task.CompletedTask;
        }

        public Task IsAvailableChanged(bool isAvailable)
        {
            _logger.LogAsync<EventHandlerService>($"CocApi isAvailable: {isAvailable}");

            return Task.CompletedTask;
        }

        public Task LeagueSizeChangeDetected(LeagueGroup leagueGroupApiModel)
        {
            _logger.LogAsync<EventHandlerService>($"League Size changed: {leagueGroupApiModel.TeamSize}");

            return Task.CompletedTask;
        }
    }
}
