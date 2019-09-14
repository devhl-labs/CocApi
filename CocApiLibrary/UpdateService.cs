using CocApiLibrary.Exceptions;
using CocApiLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CocApiLibrary
{
    internal class UpdateService
    {
        public readonly List<string> clanStrings = new List<string>();

        private readonly CocApi _cocApi;
        private bool _continueUpdatingObjects = false;
        private bool _objectsAreBeingUpdated = false;
        private const string Source = nameof(UpdateService);

        public UpdateService(CocApi cocApi)
        {
            _cocApi = cocApi;
        }

        public async Task StopUpdatingClans()
        {
            _continueUpdatingObjects = false;

            while (_objectsAreBeingUpdated)
            {
                await Task.Delay(100);
            }
        }

        public void BeginUpdatingClans()
        {
            _continueUpdatingObjects = true;

            _objectsAreBeingUpdated = true;

            Task.Run(async () =>
            {
                while (_continueUpdatingObjects)
                {
                    for (int i = 0; i < clanStrings.Count; i++)
                    {
                        await UpdateClanTryAsync(clanStrings[i]);

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
                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, Source, $"Update {clanString} starting"));

                ClanAPIModel storedClan = await _cocApi.GetClanAsync(clanString);

                ClanAPIModel downloadedClan = await _cocApi.GetClanAsync(clanString, true, false);

                storedClan.Update(_cocApi, downloadedClan);

                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, Source, $"Downloading league wars {clanString}"));

                await DownloadLeagueWarsTryAsync(storedClan);

                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, Source, $"Downloading current war {clanString}"));

                await _cocApi.GetCurrentWarAsync(storedClan.Tag);

                storedClan.AnnounceWars = true;  //we have tried to download all wars at least once, announce future wars

                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, Source, $"Updating wars {clanString}"));

                await UpdateWarsTryAsync(storedClan);

                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, Source, $"Updating villages {clanString}"));

                await UpdateVillagesTryAsync(storedClan);

                _ = _cocApi.Logger(new LogMessage(LogSeverity.Verbose, Source, $"Update {clanString} Completed"));
            }
            catch (Exception e)
            {
                _ = _cocApi.Logger(new LogMessage(LogSeverity.Warning, Source, $"Error in UpdateClanTryAsync({clanString})", e));
            }
        }



        private async Task UpdateWarsTryAsync(ClanAPIModel storedClan)
        {
            try
            {
                foreach (ICurrentWarAPIModel storedWar in storedClan.Wars.Values)
                {
                    ICurrentWarAPIModel? downloadedWar = await _cocApi.GetCurrentWarAsync(storedWar);

                    if (storedWar is CurrentWarAPIModel currentWar)
                    {
                        currentWar.Update(_cocApi, currentWar);
                    }
                    else if (storedWar is LeagueWarAPIModel leagueWar)
                    {
                        leagueWar.Update(_cocApi, leagueWar);
                    }
                }
            }
            catch (Exception e)
            {
                _ = _cocApi.Logger(new LogMessage(LogSeverity.Warning, Source, $"Error in UpdateClanTryAsync({storedClan?.Tag})", e));
            }
        }

        private async Task DownloadLeagueWarsTryAsync(ClanAPIModel storedClan)
        {
            try
            {
                if (_cocApi.DownloadLeagueWars && storedClan.DownloadLeagueWars)
                {
                    LeagueGroupAPIModel? leagueGroupAPIModel = null;

                    try
                    {
                        leagueGroupAPIModel = await _cocApi.GetLeagueGroupAsync(storedClan.Tag, true, false);
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

                    if(leagueGroupAPIModel != null)
                    {
                        foreach (var round in leagueGroupAPIModel.Rounds.EmptyIfNull())
                        {
                            foreach (var warTag in round.WarTags.EmptyIfNull().Where(w => w != "#0"))
                            {
                                ICurrentWarAPIModel leagueWar = await _cocApi.GetLeagueWarAsync(warTag, true, false);

                                if (leagueWar.Clans.Any(c => c.Tag == storedClan.Tag)) continue;

                                if (!_continueUpdatingObjects || !_cocApi.DownloadLeagueWars || !storedClan.DownloadLeagueWars)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _ = _cocApi.Logger(new LogMessage(LogSeverity.Warning, Source, $"Error in DownloadLeagueWarsTryAsync({storedClan.Tag})", e));
            }
        }

        private async Task UpdateVillagesTryAsync(ClanAPIModel storedClan)
        {
            try
            {
                if (_cocApi.DownloadVillages && storedClan.DownloadVillages)
                {
                    foreach (var village in storedClan.Members.EmptyIfNull())
                    {
                        VillageAPIModel? storedVillage;

                        VillageAPIModel? downloadedVillage;

                        try  
                        {
                            storedVillage = await _cocApi.GetVillageAsync(village.Tag);

                            downloadedVillage = await _cocApi.GetVillageAsync(village.Tag, true, false);
                        }
                        catch (Exception)
                        {
                            //there is a bug in the api where some villages that appear in the clan members list do not appear in the player end point
                            continue;
                        }

                        storedVillage.Update(_cocApi, downloadedVillage);

                        if (!_continueUpdatingObjects || !_cocApi.DownloadVillages || !storedClan.DownloadVillages)
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _ = _cocApi.Logger(new LogMessage(LogSeverity.Warning, Source, $"Error in UpdateVillagesTryAsync({storedClan.Tag})", e));
            }
        }
    }
}







