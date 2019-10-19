using CocApiLibrary.Exceptions;
using CocApiLibrary.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CocApiLibrary
{
    public class UpdateService : CommonMethods
    {
        internal List<string> ClanStrings { get; } = new List<string>();

        private readonly CocApi _cocApi;

        private bool _continueUpdatingObjects = false;
        private bool _objectsAreBeingUpdated = false;
        private readonly string _source = "UpdateService | ";

        /// <summary>
        /// This is an internal CocApiLibrary class that you do not need to instantiate.
        /// </summary>
        /// <param name="cocApi"></param>
        public UpdateService(CocApi cocApi)
        {
            _cocApi = cocApi;
        }

        /// <summary>
        /// Stops updating objects.  This may take some time to complete as the executing updates finish.
        /// </summary>
        /// <returns></returns>
        internal async Task StopUpdatingClansAsync()
        {
            _continueUpdatingObjects = false;

            while (_objectsAreBeingUpdated)
            {
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Sets the flag to mark updates should stop.
        /// </summary>
        internal void StopUpdatingClans()
        {
            _continueUpdatingObjects = false;
        }

        internal void BeginUpdatingClans()
        {
            _continueUpdatingObjects = true;

            _objectsAreBeingUpdated = true;

            Task.Run(async () =>
            {
                while (_continueUpdatingObjects)
                {
                    for (int i = 0; i < ClanStrings.Count; i++)
                    {
                        Task task = UpdateClanAsync(ClanStrings[i]);

                        await SwallowAsync(task, "{source} UpdateClanAsync({clanString})", _source, ClanStrings[i]);

                        await Task.Delay(100);

                        if (!_continueUpdatingObjects)
                        {
                            break;
                        }
                    }
                }

                _objectsAreBeingUpdated = false;
            });
        }

        private async Task UpdateClanAsync(string clanString)
        {
            Task? task;

            _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source} UpdateClanAsync({clanString})", _source, clanString);

            ClanAPIModel storedClan = await _cocApi.GetClanAsync(clanString, allowStoredItem: true, allowExpiredItem: true);

            ClanAPIModel downloadedClan = await _cocApi.GetClanAsync(clanString, allowStoredItem: true, allowExpiredItem: false);

            storedClan.Update(_cocApi, downloadedClan);

            ICurrentWarAPIModel? currentWarAPIModel = await _cocApi.GetCurrentWarOrDefaultAsync(storedClan.Tag, allowStoredItem: true, allowExpiredItem: false);

            LeagueGroupAPIModel? leagueGroupAPIModel = null;

            if (currentWarAPIModel?.State == Enums.WarState.NotInWar || currentWarAPIModel is LeagueWarAPIModel)
            {
                leagueGroupAPIModel = await DownloadLeagueGroupAsync(storedClan);

                task = DownloadLeagueWarsAsync(storedClan, leagueGroupAPIModel);

                await SwallowAsync(task, "{source} DownloadLeagueWarsAsync({clanString})", _source, clanString);
            }
                
            storedClan.AnnounceWars = true;  //We have tried to download all wars at least once, announce future wars.  This prevents all wars from being announced on startup

            task = UpdateWarsAsync(storedClan, leagueGroupAPIModel);

            await SwallowAsync(task, "{source} UpdateWarsAsync({clanString})", _source, clanString);

            task = UpdateVillagesAsync(storedClan);

            await SwallowAsync(task, "{source} UpdateVillagesAsync({clanString})", _source, clanString);
        }



        private async Task UpdateWarsAsync(ClanAPIModel storedClan, LeagueGroupAPIModel? leagueGroupAPIModel)
        {
            _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source} UpdateWarsAsync({clanTag})", _source, storedClan.Tag);

            foreach (ICurrentWarAPIModel storedWar in storedClan.Wars.Values)
            {
                ICurrentWarAPIModel? downloadedWar = await _cocApi.GetCurrentWarAsync(storedWar);

                if (storedWar is LeagueWarAPIModel leagueWar)
                {
                    leagueWar.Update(_cocApi, leagueWar, leagueGroupAPIModel);
                }
                else
                if (storedWar is CurrentWarAPIModel currentWar)
                {
                    currentWar.Update(_cocApi, currentWar, null);
                } 
            }
        }

        private async Task<LeagueGroupAPIModel?> DownloadLeagueGroupAsync(ClanAPIModel storedClan)
        {
            LeagueGroupAPIModel? leagueGroupAPIModel = null;

            if (_cocApi.IsDownloadingLeagueWars() && storedClan.DownloadLeagueWars)
            {
                _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source} DownloadLeagueGroupAsync({clanTag})", _source, storedClan.Tag);
                
                try
                {
                    leagueGroupAPIModel = await _cocApi.GetLeagueGroupAsync(storedClan.Tag, allowStoredItem: true, allowExpiredItem: false);
                }
                catch (Exception)
                {
                }
            }
            return leagueGroupAPIModel;
        }

        private async Task DownloadLeagueWarsAsync(ClanAPIModel storedClan, LeagueGroupAPIModel? leagueGroupAPIModel)
        {
            if (_cocApi.IsDownloadingLeagueWars() && storedClan.DownloadLeagueWars && leagueGroupAPIModel != null)
            {
                List<Task> tasks = new List<Task>();

                foreach (var round in leagueGroupAPIModel.Rounds.EmptyIfNull())
                {
                    tasks.Add(DownloadRoundAsync(storedClan, round));
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task DownloadRoundAsync(ClanAPIModel storedClan, RoundAPIModel round)
        {
            foreach (var warTag in round.WarTags.EmptyIfNull().Where(w => w != "#0"))
            {
                ICurrentWarAPIModel leagueWar = await _cocApi.GetLeagueWarAsync(warTag, allowStoredItem: true, allowExpiredItem: false);

                if (leagueWar.Clans.Any(c => c.Tag == storedClan.Tag)) continue;

                if (!_continueUpdatingObjects || !_cocApi.IsDownloadingLeagueWars() || !storedClan.DownloadLeagueWars)
                {
                    return;
                }
            }
        }

        private async Task UpdateVillagesAsync(ClanAPIModel storedClan)
        {
            if (_cocApi.DownloadVillages && storedClan.DownloadVillages)
            {
                _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source} UpdateVillagesAsync({clanTag})", _source, storedClan.Tag);

                List<Task> tasks = new List<Task>();

                foreach (var village in storedClan.Members.EmptyIfNull())
                {
                    tasks.Add(UpdateVillageAsync(village));
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task UpdateVillageAsync(MemberListAPIModel village)
        {
            VillageAPIModel? storedVillage;

            VillageAPIModel? downloadedVillage;

            try
            {
                storedVillage = await _cocApi.GetVillageAsync(village.Tag, allowStoredItem: true, allowExpiredItem: true);

                downloadedVillage = await _cocApi.GetVillageAsync(village.Tag, allowStoredItem: true, allowExpiredItem: false);
            }
            catch (NotFoundException)
            {
                //there is a bug in the api where some villages that appear in the clan members list do not appear in the player end point
                return;
            }

            storedVillage.Update(_cocApi, downloadedVillage);
        }
    }
}







