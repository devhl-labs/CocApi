using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.War;
using devhl.CocApi.Models.Village;
using System.Collections.Concurrent;
using System.Threading;

namespace devhl.CocApi
{
    internal sealed class UpdateService
    {
        private readonly CocApi _cocApi;

        private readonly ILogger? _logger;

        private readonly string _source = "UpdateService | ";

        private bool _continueUpdatingObjects = false;

        public UpdateService(CocApi cocApi, ILogger? logger)
        {
            _cocApi = cocApi;

            _logger = logger;
        }

        public ConcurrentBag<string> ClanStrings { get; } = new ConcurrentBag<string>();

        public bool ObjectsAreBeingUpdated { get; private set; } = false;

        public void StartUpdatingClans()
        {
            _continueUpdatingObjects = true;

            if (ObjectsAreBeingUpdated) return;

            ObjectsAreBeingUpdated = true;

            Task.Run(async () =>
            {
                try
                {
                    while (_continueUpdatingObjects)
                    {
                        foreach (string clanTag in ClanStrings)
                        {
                            await UpdateClanAsync(clanTag).ConfigureAwait(false);

                            if (!_continueUpdatingObjects) break;
                        }

                        await Task.Delay(100).ConfigureAwait(false);
                    }

                    ObjectsAreBeingUpdated = false;
                }
                catch (Exception e)
                {
                    _continueUpdatingObjects = false;

                    ObjectsAreBeingUpdated = false;

                    _logger.LogCritical(LoggingEvents.UnhandledError, "{source} {message}", _source, e.Message);

                    _cocApi.CrashDetectedEvent();

                    throw;
                }
            });
        }

        /// <summary>
        /// Sets the flag to mark updates should stop.  Returns immediately but the service will take time to stop.
        /// </summary>
        public void StopUpdatingClans()
        {
            _continueUpdatingObjects = false;
        }

        /// <summary>
        /// Stops updating objects.  This may take some time to complete as the executing updates finish.
        /// </summary>
        /// <returns></returns>
        public Task<bool> StopUpdatingClansAsync()
        {                               
            _continueUpdatingObjects = false;

            TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();

            Task<bool> task = tsc.Task;

            _ = Task.Run(async () =>
            {
                while (ObjectsAreBeingUpdated)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }

                tsc.SetResult(true);
            });


            return tsc.Task;
        }

        private void AnnounceNewWar(Clan storedClan, CurrentWar currentWarApiModel)
        {
            if (currentWarApiModel.Flags.WarAnnounced) return;

            //we only announce wars if this flag is false to avoid spamming new war events when the program starts.
            if (storedClan.AnnounceWars)
            {
                _cocApi.NewWarEvent(currentWarApiModel);
            }

            currentWarApiModel.Flags.WarAnnounced = true;
        }

        private async Task DownloadLeagueWarsAsync(Clan storedClan, ILeagueGroup? leagueGroup)
        {
            if (leagueGroup is LeagueGroup leagueGroupApiModel)
            {
                if (_cocApi.IsDownloadingLeagueWars() && storedClan.DownloadLeagueWars && leagueGroupApiModel != null)
                {
                    List<Task> tasks = new List<Task>();

                    foreach (var round in leagueGroupApiModel.Rounds.EmptyIfNull())
                    {
                        tasks.Add(DownloadRoundAsync(storedClan, round));
                    }

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
            }
        }

        private async Task DownloadRoundAsync(Clan storedClan, Round round)
        {
            foreach (var warTag in round.WarTags.EmptyIfNull().Where(w => w != "#0"))
            {
                IActiveWar? leagueWar = await _cocApi.GetLeagueWarOrDefaultAsync(warTag, allowExpiredItem: false).ConfigureAwait(false);

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

        private async Task UpdateClanAsync(string clanString)
        {
            _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source} UpdateClanAsync({clanString})", _source, clanString);

            Clan? storedClan = await _cocApi.GetClanOrDefaultAsync(clanString, allowExpiredItem: true).ConfigureAwait(false);

            Clan? downloadedClan = await _cocApi.GetClanOrDefaultAsync(clanString, allowExpiredItem: false).ConfigureAwait(false);

            if (storedClan == null || downloadedClan == null) return;

            storedClan.Update(_cocApi, downloadedClan);

            IWar? war = null;

            if (downloadedClan.IsWarLogPublic == true)
            {
                war = await _cocApi.GetCurrentWarOrDefaultAsync(storedClan.ClanTag, allowExpiredItem: false).ConfigureAwait(false) as IActiveWar;
            }

            ILeagueGroup? leagueGroup = null;

            if (_cocApi.IsDownloadingLeagueWars() && (war == null || war is LeagueWar))
            {
                leagueGroup = await _cocApi.GetLeagueGroupOrDefaultAsync(storedClan.ClanTag).ConfigureAwait(false);

                await DownloadLeagueWarsAsync(storedClan, leagueGroup).ConfigureAwait(false);
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
                    AnnounceNewWar(storedClan, storedWar);
                }
            }
            
            await UpdateWarsAsync(storedClan, leagueGroup).ConfigureAwait(false);

            await UpdateVillagesAsync(storedClan).ConfigureAwait(false);

            downloadedClan.AnnounceWars = storedClan.AnnounceWars;

            downloadedClan.DownloadLeagueWars = storedClan.DownloadLeagueWars;

            downloadedClan.DownloadVillages = storedClan.DownloadVillages;

            downloadedClan.Wars = storedClan.Wars;
        }

        private async Task UpdateVillageAsync(ClanVillage village)
        {
            Village? storedVillage = await _cocApi.GetVillageOrDefaultAsync(village.VillageTag, allowExpiredItem: true).ConfigureAwait(false);

            Village? downloadedVillage = await _cocApi.GetVillageOrDefaultAsync(village.VillageTag, allowExpiredItem: false).ConfigureAwait(false);

            if (storedVillage != null && downloadedVillage != null)
            {
                storedVillage.Update(_cocApi, downloadedVillage);
            }
        }

        private async Task UpdateVillagesAsync(Clan storedClan)
        {
            if (_cocApi.DownloadVillages && storedClan.DownloadVillages)
            {
                List<Task> tasks = new List<Task>();

                foreach (var village in storedClan.Villages.EmptyIfNull())
                {
                    tasks.Add(UpdateVillageAsync(village));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task UpdateWarsAsync(Clan storedClan, ILeagueGroup? leagueGroup)
        {
            int numberOfWars = storedClan.Wars.Count;

            for (int i = 0; i < numberOfWars; i++)
            {
                IActiveWar storedWar;

                storedWar = storedClan.Wars.ElementAt(i).Value;

                IActiveWar? downloadedWar = await _cocApi.GetCurrentWarOrDefaultAsync(storedWar).ConfigureAwait(false);

                if (storedWar is LeagueWar leagueWar)
                {
                    leagueWar.Update(_cocApi, leagueWar, leagueGroup);
                }
                else if (storedWar is CurrentWar currentWar)
                {
                    currentWar.Update(_cocApi, downloadedWar, null);
                }

                if (downloadedWar is CurrentWar currentWar1)
                {
                    currentWar1.Flags = storedWar.Flags;

                    storedClan.Wars.AddOrUpdate(storedWar.WarId, currentWar1, (_, war2) => {

                        if (downloadedWar.UpdatedAtUtc > war2.UpdatedAtUtc) return currentWar1;

                        return war2;
                    });
                }
            }
        }
    }
}







