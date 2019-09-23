using CocApiLibrary.Exceptions;
using CocApiLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CocApiLibrary
{
    internal class UpdateService
    {
        public List<string> ClanStrings { get; } = new List<string>();

        private readonly CocApi _cocApi;
        private bool _continueUpdatingObjects = false;
        private bool _objectsAreBeingUpdated = false;
        private const string _source = nameof(UpdateService);

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

            while (_objectsAreBeingUpdated)
            {
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Sets the flag to mark updates should stop.
        /// </summary>
        public void StopUpdatingClans()
        {
            _continueUpdatingObjects = false;
        }

        public void BeginUpdatingClans()
        {
            _continueUpdatingObjects = true;

            _objectsAreBeingUpdated = true;

            Task.Run(async () =>
            {
                while (_continueUpdatingObjects)
                {
                    for (int i = 0; i < ClanStrings.Count; i++)
                    {
                        await UpdateClanTryAsync(ClanStrings[i]);

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

        private async Task UpdateClanTryAsync(string clanString)
        {
            try
            {
                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, _source, $"Update {clanString} starting"));

                ClanAPIModel storedClan = await _cocApi.GetClanAsync(clanString, allowStoredItem: true, allowExpiredItem: true);

                ClanAPIModel downloadedClan = await _cocApi.GetClanAsync(clanString, allowStoredItem: true, allowExpiredItem: false);

                storedClan.Update(_cocApi, downloadedClan);

                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, _source, $"Downloading league wars {clanString}"));

                await DownloadLeagueWarsTryAsync(storedClan);

                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, _source, $"Downloading current war {clanString}"));

                await _cocApi.GetCurrentWarAsync(storedClan.Tag, allowStoredItem: true, allowExpiredItem: false);

                storedClan.AnnounceWars = true;  //We have tried to download all wars at least once, announce future wars.  This prevents all wars from being announced on startup

                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, _source, $"Updating wars {clanString}"));

                await UpdateWarsTryAsync(storedClan);

                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, _source, $"Updating villages {clanString}"));

                await UpdateVillagesTryAsync(storedClan);

                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, _source, $"Update {clanString} Completed"));
            }
            catch (Exception e)
            {
                _ = _cocApi.Logger(new LogMessage(LogSeverity.Warning, _source, $"Error in UpdateClanTryAsync({clanString})", e));
            }
        }



        private async Task UpdateWarsTryAsync(ClanAPIModel storedClan)
        {
            try
            {
                foreach (ICurrentWarAPIModel storedWar in storedClan.Wars.Values)
                {
                    ICurrentWarAPIModel? downloadedWar = await _cocApi.GetCurrentWarAsync(storedWar);

                    if (storedWar is LeagueWarAPIModel leagueWar)
                    {
                        leagueWar.Update(_cocApi, leagueWar);
                    }
                    else
                    if (storedWar is CurrentWarAPIModel currentWar)
                    {
                        currentWar.Update(_cocApi, currentWar);
                    } 
                }
            }
            catch (Exception e)
            {
                _ = _cocApi.Logger(new LogMessage(LogSeverity.Warning, _source, $"Error in UpdateClanTryAsync({storedClan?.Tag})", e));
            }
        }

        private async Task DownloadLeagueWarsTryAsync(ClanAPIModel storedClan)
        {
            try
            {
                if (_cocApi.IsDownloadingLeagueWars() && storedClan.DownloadLeagueWars)
                {
                    LeagueGroupAPIModel? leagueGroupAPIModel = null;

                    try
                    {
                        leagueGroupAPIModel = await _cocApi.GetLeagueGroupAsync(storedClan.Tag, allowStoredItem: true, allowExpiredItem: false);
                    }
                    catch (NotFoundException) 
                    {
                        //clans not in a league war will return this error
                        return;
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    if (leagueGroupAPIModel != null)
                    {
                        List<Task> tasks = new List<Task>();

                        foreach (var round in leagueGroupAPIModel.Rounds.EmptyIfNull())
                        {
                            tasks.Add(DownloadRoundTryAsync(storedClan, round));
                        }

                        await Task.WhenAll(tasks);
                    }
                }
            }
            catch (Exception e)
            {
                _ = _cocApi.Logger(new LogMessage(LogSeverity.Warning, _source, $"Error in DownloadLeagueWarsTryAsync({storedClan.Tag})", e));
            }
        }

        private async Task DownloadRoundTryAsync(ClanAPIModel storedClan, RoundAPIModel round)
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

        private async Task UpdateVillagesTryAsync(ClanAPIModel storedClan)
        {          
            try
            {

                if (_cocApi.DownloadVillages && storedClan.DownloadVillages)
                {
                    List<Task> tasks = new List<Task>();

                    //tasks.Add(UpdateVillageAsync(storedClan.Members.First())); //todo

                    //tasks.Add(UpdateVillageAsync(storedClan.Members[1]));

                    foreach (var village in storedClan.Members.EmptyIfNull())
                    {
                        tasks.Add(UpdateVillageAsync(village));
                    }

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception e)
            {
                _ = _cocApi.Logger(new LogMessage(LogSeverity.Warning, _source, $"Error in UpdateVillagesTryAsync({storedClan.Tag})", e));
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







