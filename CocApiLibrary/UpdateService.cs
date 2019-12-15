using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.War;
using devhl.CocApi.Models.Village;

namespace devhl.CocApi
{
    internal sealed class UpdateService
    {
        public List<string> ClanStrings { get; } = new List<string>();

        private readonly CocApi _cocApi;

        private bool _continueUpdatingObjects = false;
        internal bool ObjectsAreBeingUpdated { get; private set; } = false;

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
        public async Task StopUpdatingClansAsync()
        {
            _continueUpdatingObjects = false;

            while (ObjectsAreBeingUpdated)
            {
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Sets the flag to mark updates should stop.  Returns immediately but the service will take time to stop.
        /// </summary>
        public void StopUpdatingClans()
        {
            _continueUpdatingObjects = false;
        }

        public void StartUpdatingClans()
        {
            _continueUpdatingObjects = true;

            ObjectsAreBeingUpdated = true;

            Task.Run(async () =>
            {
                try
                {
                    while (_continueUpdatingObjects)
                    {
                        for (int i = 0; i < ClanStrings.Count; i++)
                        {
                            await UpdateClanAsync(ClanStrings[i]);
                            
                            if (i == 0) await Task.Delay(100);

                            if (!_continueUpdatingObjects) break;
                        }
                    }

                    ObjectsAreBeingUpdated = false;
                }
                catch (Exception e)
                {
                    _continueUpdatingObjects = false;

                    ObjectsAreBeingUpdated = false;

                    Logger.LogCritical(LoggingEvents.UnhandledError, "{source} {message}", _source, e.Message);

                    _cocApi.CrashDetectedEvent();

                    throw;
                }
            });
        }

        public ILogger? Logger { get; set; }

        private async Task UpdateClanAsync(string clanString)
        {
            _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source} UpdateClanAsync({clanString})", _source, clanString);

            ClanApiModel? storedClan = await _cocApi.GetClanOrDefaultAsync(clanString, allowStoredItem: true, allowExpiredItem: true);

            ClanApiModel? downloadedClan = await _cocApi.GetClanOrDefaultAsync(clanString, allowStoredItem: true, allowExpiredItem: false);

            if (storedClan == null || downloadedClan == null) return;

            storedClan.Update(_cocApi, downloadedClan);

            IWar? war = null;

            if (downloadedClan.IsWarLogPublic == true)
            {
                war = await _cocApi.GetCurrentWarOrDefaultAsync(storedClan.ClanTag, allowStoredItem: true, allowExpiredItem: false) as ICurrentWarApiModel;
            }

            ILeagueGroup? leagueGroup = null;

            if (_cocApi.IsDownloadingLeagueWars() && (war == null || war is LeagueWarApiModel))
            {
                leagueGroup = await _cocApi.GetLeagueGroupOrDefaultAsync(storedClan.ClanTag);

                await DownloadLeagueWarsAsync(storedClan, leagueGroup);
            }
            
            if (storedClan.AnnounceWars == false)
            {
                //We have tried to download all wars at least once, announce future wars.  This prevents all wars from being announced on startup
                storedClan.AnnounceWars = true;  

                foreach(var storedWar in storedClan.Wars.Values)
                {
                    storedWar.Flags.WarAnnounced = true;
                }
            }
            else
            {
                foreach(var storedWar in storedClan.Wars.Values)
                {
                    AnnounceNewWar(storedClan, (CurrentWarApiModel)storedWar);
                }
            }
            
            await UpdateWarsAsync(storedClan, leagueGroup);

            await UpdateVillagesAsync(storedClan);

            downloadedClan.AnnounceWars = storedClan.AnnounceWars;

            downloadedClan.DownloadLeagueWars = storedClan.DownloadLeagueWars;

            downloadedClan.DownloadVillages = storedClan.DownloadVillages;

            downloadedClan.Wars = storedClan.Wars;

            //Update service must update the dictionary so the old state is not lost and the events can be fired
            lock (_cocApi._allClansLock)
            {
                _cocApi.AllClans[downloadedClan.ClanTag] = downloadedClan;
            }
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

                ICurrentWarApiModel? downloadedWar = await _cocApi.GetCurrentWarOrDefaultAsync(storedWar);

                if (storedWar is LeagueWarApiModel leagueWar)
                {
                    leagueWar.Update(_cocApi, leagueWar, leagueGroup);
                }
                else if (storedWar is CurrentWarApiModel currentWar)
                {
                    currentWar.Update(_cocApi, downloadedWar, null);
                }

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
                ICurrentWarApiModel? leagueWar = await _cocApi.GetLeagueWarOrDefaultAsync(warTag, allowStoredItem: true, allowExpiredItem: false);

                if (leagueWar == null) continue;

                if (leagueWar.Clans.Any(c => c.ClanTag == storedClan.ClanTag))
                {
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
            VillageApiModel? storedVillage = await _cocApi.GetVillageOrDefaultAsync(village.VillageTag, allowStoredItem: true, allowExpiredItem: true);

            VillageApiModel? downloadedVillage = await _cocApi.GetVillageOrDefaultAsync(village.VillageTag, allowStoredItem: true, allowExpiredItem: false);

            if (storedVillage != null && downloadedVillage != null)
            {
                storedVillage.Update(_cocApi, downloadedVillage);
            }
        }
    }
}







