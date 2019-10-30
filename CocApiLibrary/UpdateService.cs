using CocApiLibrary.Exceptions;
using CocApiLibrary.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CocApiLibrary
{
    public sealed class UpdateService : SwallowDelegates
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

                        await SwallowAsync(task, $"{nameof(UpdateClanAsync)}({ClanStrings[i]})");

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

            ClanApiModel storedClan = await _cocApi.GetClanAsync(clanString, allowStoredItem: true, allowExpiredItem: true);

            ClanApiModel downloadedClan = await _cocApi.GetClanAsync(clanString, allowStoredItem: true, allowExpiredItem: false);

            storedClan.Update(_cocApi, downloadedClan);

            ICurrentWarApiModel? currentWarApiModel = await _cocApi.GetCurrentWarOrDefaultAsync(storedClan.ClanTag, allowStoredItem: true, allowExpiredItem: false);

            LeagueGroupApiModel? leagueGroupApiModel = null;

            if (currentWarApiModel?.State == Enums.WarState.NotInWar || currentWarApiModel is LeagueWarApiModel)
            {
                leagueGroupApiModel = await DownloadLeagueGroupAsync(storedClan);

                task = DownloadLeagueWarsAsync(storedClan, leagueGroupApiModel);

                await SwallowAsync(task, $"{nameof(DownloadLeagueWarsAsync)}({ClanStrings})");
            }
                
            storedClan.AnnounceWars = true;  //We have tried to download all wars at least once, announce future wars.  This prevents all wars from being announced on startup

            task = UpdateWarsAsync(storedClan, leagueGroupApiModel);

            await SwallowAsync(task, $"{nameof(UpdateWarsAsync)}({ClanStrings})");

            task = UpdateVillagesAsync(storedClan);

            await SwallowAsync(task, $"{nameof(UpdateVillageAsync)}({clanString})");
        }



        private async Task UpdateWarsAsync(ClanApiModel storedClan, LeagueGroupApiModel? leagueGroupApiModel)
        {
            _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source} UpdateWarsAsync({clanTag})", _source, storedClan.ClanTag);

            foreach (ICurrentWarApiModel storedWar in storedClan.Wars.Values)
            {
                ICurrentWarApiModel? downloadedWar = await _cocApi.GetCurrentWarAsync(storedWar);

                if (storedWar is LeagueWarApiModel leagueWar)
                {
                    leagueWar.Update(_cocApi, leagueWar, leagueGroupApiModel);
                }
                else
                if (storedWar is CurrentWarApiModel currentWar)
                {
                    currentWar.Update(_cocApi, currentWar, null);
                } 
            }
        }

        private async Task<LeagueGroupApiModel?> DownloadLeagueGroupAsync(ClanApiModel storedClan)
        {
            LeagueGroupApiModel? leagueGroupApiModel = null;

            if (_cocApi.IsDownloadingLeagueWars() && storedClan.DownloadLeagueWars)
            {
                _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source} DownloadLeagueGroupAsync({clanTag})", _source, storedClan.ClanTag);
                
                try
                {
                    leagueGroupApiModel = await _cocApi.GetLeagueGroupAsync(storedClan.ClanTag, allowStoredItem: true, allowExpiredItem: false);
                }
                catch (Exception)
                {
                }
            }
            return leagueGroupApiModel;
        }

        private async Task DownloadLeagueWarsAsync(ClanApiModel storedClan, LeagueGroupApiModel? leagueGroupApiModel)
        {
            if (_cocApi.IsDownloadingLeagueWars() && storedClan.DownloadLeagueWars && leagueGroupApiModel != null)
            {
                List<Task> tasks = new List<Task>();

                foreach (var round in leagueGroupApiModel.Rounds.EmptyIfNull())
                {
                    tasks.Add(DownloadRoundAsync(storedClan, round));
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task DownloadRoundAsync(ClanApiModel storedClan, RoundApiModel round)
        {
            foreach (var warTag in round.WarTags.EmptyIfNull().Where(w => w != "#0"))
            {
                ICurrentWarApiModel leagueWar = await _cocApi.GetLeagueWarAsync(warTag, allowStoredItem: true, allowExpiredItem: false);

                if (leagueWar.Clans.Any(c => c.ClanTag == storedClan.ClanTag)) continue;

                if (!_continueUpdatingObjects || !_cocApi.IsDownloadingLeagueWars() || !storedClan.DownloadLeagueWars)
                {
                    return;
                }
            }
        }

        private async Task UpdateVillagesAsync(ClanApiModel storedClan)
        {
            if (_cocApi.DownloadVillages && storedClan.DownloadVillages)
            {
                _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source} UpdateVillagesAsync({clanTag})", _source, storedClan.ClanTag);

                List<Task> tasks = new List<Task>();

                foreach (var village in storedClan.Villages.EmptyIfNull())
                {
                    tasks.Add(UpdateVillageAsync(village));
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task UpdateVillageAsync(ClanVillageApiModel village)
        {
            VillageApiModel? storedVillage;

            VillageApiModel? downloadedVillage;

            try
            {
                storedVillage = await _cocApi.GetVillageAsync(village.VillageTag, allowStoredItem: true, allowExpiredItem: true);

                downloadedVillage = await _cocApi.GetVillageAsync(village.VillageTag, allowStoredItem: true, allowExpiredItem: false);
            }
            catch (NotFoundException)
            {
                //there is a bug in the api where some villages that appear in the clan village list do not appear in the player end point
                return;
            }

            storedVillage.Update(_cocApi, downloadedVillage);
        }
    }
}







