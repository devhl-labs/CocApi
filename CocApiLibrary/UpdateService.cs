using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.War;
using devhl.CocApi.Models.Village;
using System.Collections.Concurrent;

namespace devhl.CocApi
{
    internal sealed class UpdateService
    {
        private readonly CocApi _cocApi;

        private bool _continueUpdatingObjects = false;

        public UpdateService(CocApi cocApi)
        {
            _cocApi = cocApi;
        }

        public ConcurrentBag<string> ClanStrings { get; } = new ConcurrentBag<string>();

        public bool ObjectsAreBeingUpdated { get; private set; } = false;

        private bool _firstPassComplete = false;

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

                        _firstPassComplete = true;

                        await Task.Delay(100).ConfigureAwait(false);
                    }

                    ObjectsAreBeingUpdated = false;

                    //_ = _cocApi.Logger?.LogAsync<CocApi>(logLevel: LogLevel.Information, loggingEvent: LoggingEvent.UpdateServiceEnding);

                    _cocApi.LogEvent<CocApi>(logLevel: LogLevel.Information, loggingEvent: LoggingEvent.UpdateServiceEnding);
                }
                catch (Exception e)
                {
                    _continueUpdatingObjects = false;

                    ObjectsAreBeingUpdated = false;

                    //_ = _cocApi.Logger?.LogAsync<UpdateService>(e, LogLevel.Critical, LoggingEvent.CrashDetected);

                    _cocApi.LogEvent<UpdateService>(e, LogLevel.Critical, LoggingEvent.CrashDetected);

                    _cocApi.CrashDetectedEvent();

                    throw;
                }
            });

            //_ = _cocApi.Logger?.LogAsync<CocApi>(logLevel: LogLevel.Information, loggingEvent: LoggingEvent.UpdateServiceStarted);

            _cocApi.LogEvent<CocApi>(logLevel: LogLevel.Information, loggingEvent: LoggingEvent.UpdateServiceStarted);
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

        private void AnnounceNewWars(Clan storedClan)
        {
            List<CurrentWar> currentWars = new List<CurrentWar>();

            if (!_firstPassComplete)
            {
                foreach(var kvp in storedClan.Wars)
                {
                    kvp.Value.Flags.WarAnnounced = true;

                    currentWars.Add(kvp.Value);
                }

                _cocApi.NewWarsEvent(currentWars);

                return;
            }

            foreach (var kvp in storedClan.Wars)
            {
                lock (kvp.Value._announceWarLock)
                {
                    if (kvp.Value.Flags.WarAnnounced) continue;

                    _cocApi.NewWarEvent(kvp.Value);

                    kvp.Value.Flags.WarAnnounced = true;
                }
            }
        }

        private async Task DownloadLeagueWarsAsync(Clan storedClan, ILeagueGroup? iLeagueGroup)
        {
            if (iLeagueGroup is LeagueGroup leagueGroup && _cocApi.IsDownloadingLeagueWars() && storedClan.DownloadLeagueWars)
            {
                List<Task> tasks = new List<Task>();

                foreach (var round in leagueGroup.Rounds.EmptyIfNull())
                {
                    tasks.Add(DownloadRoundAsync(storedClan, round));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task DownloadRoundAsync(Clan storedClan, Round round)
        {
            foreach (string warTag in round.WarTags.EmptyIfNull().Where(w => w != "#0"))
            {
                LeagueWar? leagueWar = await _cocApi.GetLeagueWarOrDefaultAsync(warTag, allowExpiredItem: false).ConfigureAwait(false);

                if (leagueWar == null) continue;

                if (leagueWar.WarClans.Any(c => c.ClanTag == storedClan.ClanTag))
                    return;

                if (!_continueUpdatingObjects || !_cocApi.IsDownloadingLeagueWars() || !storedClan.DownloadLeagueWars) return;
            }
        }

        private async Task UpdateClanAsync(string clanString)
        {
            //_ = _cocApi.Logger?.LogAsync<UpdateService>($"Updating clan {clanString}", LogLevel.Trace, LoggingEvent.UpdatingClan);

            _cocApi.LogEvent<UpdateService>($"Updating clan {clanString}", LogLevel.Trace, LoggingEvent.UpdatingClan);

            Clan? storedClan = await _cocApi.GetClanOrDefaultAsync(clanString, allowExpiredItem: true).ConfigureAwait(false);

            Clan? downloadedClan = await _cocApi.GetClanOrDefaultAsync(clanString, allowExpiredItem: false).ConfigureAwait(false);

            if (storedClan == null || downloadedClan == null) return;

            storedClan.Update(_cocApi, downloadedClan);

            IWar? war = null;

            if (downloadedClan.IsWarLogPublic == true && _cocApi.DownloadCurrentWar && storedClan.DownloadCurrentWar)
            {
                war = await _cocApi.GetCurrentWarOrDefaultAsync(storedClan.ClanTag, allowExpiredItem: false).ConfigureAwait(false);
            }

            if ((war == null || war is LeagueWar || war is PrivateWarLog || war is NotInWar) && storedClan.DownloadLeagueWars && _cocApi.IsDownloadingLeagueWars())
            {
                ILeagueGroup? leagueGroup = await _cocApi.GetLeagueGroupOrDefaultAsync(storedClan.ClanTag).ConfigureAwait(false);

                await DownloadLeagueWarsAsync(storedClan, leagueGroup).ConfigureAwait(false);
            }

            AnnounceNewWars(storedClan);

            await UpdateWarsAsync(storedClan).ConfigureAwait(false);

            await UpdateVillagesAsync(storedClan).ConfigureAwait(false);

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
            if (_cocApi.DownloadVillages && storedClan.DownloadVillages && (_cocApi.DownloadCurrentWar == false || _firstPassComplete))
            {
                List<Task> tasks = new List<Task>();

                foreach (var village in storedClan.Villages.EmptyIfNull())
                {
                    tasks.Add(UpdateVillageAsync(village));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task UpdateWarsAsync(Clan storedClan)
        {
            int numberOfWars = storedClan.Wars.Count;

            for (int i = 0; i < numberOfWars; i++)
            {
                CurrentWar storedWar = storedClan.Wars.ElementAt(i).Value;

                CurrentWar? downloadedWar = await _cocApi.GetCurrentWarOrDefaultAsync(storedWar).ConfigureAwait(false);

                if (storedWar is LeagueWar storedLeagueWar && downloadedWar is LeagueWar downloadedLeagueWar)
                {
                    storedLeagueWar.Update(_cocApi, downloadedLeagueWar);
                }
                else
                {
                    storedWar.Update(_cocApi, downloadedWar);
                }

                if (downloadedWar == null) continue;

                downloadedWar.Flags = storedWar.Flags;

                storedClan.Wars.AddOrUpdate(storedWar.WarKey, downloadedWar, (_, war2) =>
                {
                    if (downloadedWar.DownloadedAtUtc > war2.DownloadedAtUtc)
                        return downloadedWar;

                    return war2;
                });

                Clan? clan = _cocApi.GetClanOrDefault(downloadedWar.WarClans.First(c => c.ClanTag != storedClan.ClanTag).ClanTag);

                if (clan == null) continue;

                clan.Wars.AddOrUpdate(storedWar.WarKey, downloadedWar, (_, war2) =>
                {
                    if (downloadedWar.DownloadedAtUtc > war2.DownloadedAtUtc) return downloadedWar;

                    return war2;
                });
            }
        }
    }
}







