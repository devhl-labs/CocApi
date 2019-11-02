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

            ICurrentWarApiModel? currentWarApiModel = null;

            if (downloadedClan.IsWarLogPublic)
            {
                currentWarApiModel = await _cocApi.GetCurrentWarOrDefaultAsync(storedClan.ClanTag, allowStoredItem: true, allowExpiredItem: false) as ICurrentWarApiModel;
            }

            if (currentWarApiModel is CurrentWarApiModel currentWar)
            {
                storedClan.Wars.TryAdd(currentWar.WarId, currentWar);
            }
            ILeagueGroup? leagueGroup = null;

            if (_cocApi.IsDownloadingLeagueWars() && (currentWarApiModel == null || currentWarApiModel is LeagueWarApiModel))
            {
                leagueGroup = await _cocApi.GetLeagueGroupAsync(storedClan.ClanTag);

                task = DownloadLeagueWarsAsync(storedClan, leagueGroup);

                await SwallowAsync(task, $"{nameof(DownloadLeagueWarsAsync)}({ClanStrings})");
            }
            
            if (storedClan.AnnounceWars == false)
            {
                //We have tried to download all wars at least once, announce future wars.  This prevents all wars from being announced on startup
                storedClan.AnnounceWars = true;  

                foreach(var war in storedClan.Wars.Values)
                {
                    war.Flags.WarAnnounced = true;
                }
            }
            else
            {
                foreach(var war in storedClan.Wars.Values)
                {
                    AnnounceNewWar(storedClan, (CurrentWarApiModel) war);
                }
            }
            

            task = UpdateWarsAsync(storedClan, leagueGroup);

            await SwallowAsync(task, $"{nameof(UpdateWarsAsync)}({ClanStrings})");

            task = UpdateVillagesAsync(storedClan);

            await SwallowAsync(task, $"{nameof(UpdateVillageAsync)}({clanString})");

            downloadedClan.AnnounceWars = storedClan.AnnounceWars;

            downloadedClan.DownloadLeagueWars = storedClan.DownloadLeagueWars;

            downloadedClan.DownloadVillages = storedClan.DownloadVillages;

            downloadedClan.Wars = storedClan.Wars;

            _cocApi.AllClans[downloadedClan.ClanTag] = downloadedClan;
        }

        private void AnnounceNewWar(ClanApiModel storedClan, CurrentWarApiModel currentWarApiModel)
        {
            lock (currentWarApiModel.NewWarLock)
            {
                if (currentWarApiModel.Flags.WarAnnounced) return;

                //we only announce wars if this flag is false to avoid spamming new war events when the program starts.
                if (storedClan.AnnounceWars)
                {
                    _cocApi.NewWarEvent(currentWarApiModel);
                }

                currentWarApiModel.Flags.WarAnnounced = true;
            }
        }

        private async Task UpdateWarsAsync(ClanApiModel storedClan, ILeagueGroup? leagueGroup)
        {
            for (int i = 0; i < storedClan.Wars.Count; i++)
            {
                var storedWar = storedClan.Wars.ElementAt(i).Value;

                IWar? downloadedWar = await _cocApi.GetCurrentWarOrDefaultAsync(storedWar);

                //if (storedWar is LeagueWarApiModel leagueWar)
                //{
                //    leagueWar.Update(_cocApi, leagueWar, leagueGroup);
                //}
                //else
                //if (storedWar is CurrentWarApiModel currentWar)
                //{
                ((CurrentWarApiModel) storedWar).Update(_cocApi, downloadedWar, null);
                //}

                if (downloadedWar is ICurrentWarApiModel downloadedCurrentWar)
                {
                    downloadedCurrentWar.Flags = storedWar.Flags;

                    storedClan.Wars[storedWar.WarId] = downloadedCurrentWar;
                }
            }
        }

        private async Task DownloadLeagueWarsAsync(ClanApiModel storedClan, ILeagueGroup? leagueGroup)
        {
            if (leagueGroup is LeagueGroupApiModel leagueGroupApiModel)
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
        }

        private async Task DownloadRoundAsync(ClanApiModel storedClan, RoundApiModel round)
        {
            foreach (var warTag in round.WarTags.EmptyIfNull().Where(w => w != "#0"))
            {
                ICurrentWarApiModel leagueWar = await _cocApi.GetLeagueWarAsync(warTag, allowStoredItem: true, allowExpiredItem: false);

                if (leagueWar.Clans.Any(c => c.ClanTag == storedClan.ClanTag))
                {
                    storedClan.Wars.TryAdd(leagueWar.WarId, leagueWar);

                    continue;
                }

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
                //_cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source} UpdateVillagesAsync({clanTag})", _source, storedClan.ClanTag);

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







