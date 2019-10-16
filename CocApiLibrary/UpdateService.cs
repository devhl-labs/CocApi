using CocApiLibrary.Exceptions;
using CocApiLibrary.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
                _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source}: Beginning to update clan: {clanTag}", _source, clanString);


                ClanAPIModel storedClan = await _cocApi.GetClanAsync(clanString, allowStoredItem: true, allowExpiredItem: true);

                ClanAPIModel downloadedClan = await _cocApi.GetClanAsync(clanString, allowStoredItem: true, allowExpiredItem: false);

                storedClan.Update(_cocApi, downloadedClan);

                _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source}: Beginning to download current war: {clanTag}", _source, clanString);

                ICurrentWarAPIModel? currentWarAPIModel = await _cocApi.GetCurrentWarOrDefaultAsync(storedClan.Tag, allowStoredItem: true, allowExpiredItem: false);

                LeagueGroupAPIModel? leagueGroupAPIModel = null;

                if (currentWarAPIModel?.State == Enums.WarState.NotInWar || currentWarAPIModel is LeagueWarAPIModel)
                {
                    _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source}: Begin downloading league wars: {clanTag}", _source, clanString);

                    leagueGroupAPIModel = await DownloadLeagueGroupTryAsync(storedClan); 

                    await DownloadLeagueWarsTryAsync(storedClan, leagueGroupAPIModel);
                }
                
                storedClan.AnnounceWars = true;  //We have tried to download all wars at least once, announce future wars.  This prevents all wars from being announced on startup

                _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source}: Beginning to update wars {clanTag}", _source, clanString);

                await UpdateWarsTryAsync(storedClan, leagueGroupAPIModel);

                _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source}: Beginning to update villages {clanTag}", _source, clanString);

                await UpdateVillagesTryAsync(storedClan);

                _cocApi.Logger?.LogDebug(LoggingEvents.UpdatingClan, "{source}: Finished updating clan {clanTag}", _source, clanString);
            }
            catch (Exception e)
            {
                _cocApi.Logger?.LogWarning(e, "{source}: Error in UpdateClanTryAsync({clanTag}", _source, clanString);
            }
        }



        private async Task UpdateWarsTryAsync(ClanAPIModel storedClan, LeagueGroupAPIModel? leagueGroupAPIModel)
        {
            try
            {
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
            catch (Exception e)
            {
                _cocApi.Logger?.LogWarning(e, "{source}: Error in UpdateClanTryAsync{clanTag}", _source, storedClan.Tag);
            }
        }

        private async Task<LeagueGroupAPIModel?> DownloadLeagueGroupTryAsync(ClanAPIModel storedClan)
        {
            LeagueGroupAPIModel? leagueGroupAPIModel = null;

            if (_cocApi.IsDownloadingLeagueWars() && storedClan.DownloadLeagueWars)
            {
                try
                {
                    leagueGroupAPIModel = await _cocApi.GetLeagueGroupAsync(storedClan.Tag, allowStoredItem: true, allowExpiredItem: false);
                }
                catch (Exception)
                {
                    _cocApi.Logger?.LogWarning("{source}: Error in DownloadLeagueGroupTryAsync{clanTag}", _source, storedClan.Tag);
                }
            }
            return leagueGroupAPIModel;
        }

        private async Task DownloadLeagueWarsTryAsync(ClanAPIModel storedClan, LeagueGroupAPIModel? leagueGroupAPIModel)
        {
            try
            {
                if (_cocApi.IsDownloadingLeagueWars() && storedClan.DownloadLeagueWars && leagueGroupAPIModel != null)
                {
                    List<Task> tasks = new List<Task>();

                    foreach (var round in leagueGroupAPIModel.Rounds.EmptyIfNull())
                    {
                        tasks.Add(DownloadRoundTryAsync(storedClan, round));
                    }

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception e)
            {
                _cocApi.Logger?.LogWarning(e, "{source}: Error in DownloadLeagueWarsTryAsync{clanTag}", _source, storedClan.Tag);
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

                    foreach (var village in storedClan.Members.EmptyIfNull())
                    {
                        tasks.Add(UpdateVillageAsync(village));
                    }

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception e)
            {
                _cocApi.Logger?.LogWarning(e, "{source}: Error in UPdateVillagesTryAsync{clanTag}", _source, storedClan.Tag);
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







