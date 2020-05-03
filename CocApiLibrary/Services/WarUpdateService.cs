//using devhl.CocApi.Models.Clan;
//using devhl.CocApi.Models.War;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace devhl.CocApi
//{
//    public class WarUpdateService
//    {
//        private readonly CocApi _cocApi;

//        private bool _stopRequested = false;

//        public WarUpdateService(CocApi cocApi) => _cocApi = cocApi;

//        public bool UpdatingWars { get; private set; } = false;

//        public void StopUpdating() => _stopRequested = false;

//        public Task StopUpdatingAsync()
//        {
//            _stopRequested = false;

//            TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();

//            Task task = tsc.Task;

//            _ = Task.Run(async () =>
//            {
//                while (UpdatingWars)
//                    await Task.Delay(100).ConfigureAwait(false);

//                tsc.SetResult(true);
//            });

//            return tsc.Task;
//        }

//        public void Update()
//        {
//            _stopRequested = false;

//            if (UpdatingWars)
//                return;

//            UpdatingWars = true;

//            Task.Run(async () =>
//            {
//                try
//                {
//                    while (_stopRequested == false)
//                    {
//                        foreach (Clan clan in _cocApi.Clans.CachedClans.Values.Where(c => c != null))
//                        {
//                            await PopulateWars(clan).ConfigureAwait(false);

//                            if (_stopRequested)
//                                break;
//                        }

//                        foreach (CurrentWar storedWar in _cocApi.Wars.WatchedWars.Values.Where(c => c != null))
//                        {
//                            await Update(storedWar).ConfigureAwait(false);

//                            if (_stopRequested)
//                                break;
//                        }

//                        await Task.Delay(50);
//                    }

//                    UpdatingWars = false;

//                    _cocApi.LogEvent<WarUpdateService>(logLevel: LogLevel.Information, loggingEvent: LoggingEvent.WarUpdateEnded);
//                }
//                catch (Exception e)
//                {
//                    _stopRequested = false;

//                    UpdatingWars = false;

//                    _cocApi.LogEvent<WarUpdateService>(e, LogLevel.Critical, LoggingEvent.CrashDetected);

//                    _cocApi.WarUpdaterCrashDetectedEvent();

//                    throw e;
//                }
//            });
//        }

//        private async Task PopulateWars(Clan clan)
//        {
//            if (clan.DownloadCurrentWar && clan.IsWarLogPublic)
//            {
//                IWar? war = await _cocApi.Wars.GetCurrentWarAsync(clan.ClanTag, allowExpiredItem: false).ConfigureAwait(false);

//                if (war is CurrentWar currentWar)
//                    _cocApi.Wars.WatchedWars.TryAdd(currentWar.WarKey, currentWar);
//            }

//            if (clan.DownloadLeagueWars && _cocApi.Wars.IsDownloadingLeagueWars())
//            {
//                List<LeagueWar>? leagueWars = await _cocApi.Wars.GetLeagueWarsAsync(clan.ClanTag, false).ConfigureAwait(false);

//                foreach (LeagueWar leagueWar in leagueWars.EmptyIfNull())
//                {
//                    _cocApi.Wars.WatchedWars.TryAdd(leagueWar.WarKey, leagueWar);
//                }
//            }
//        }

//        private async Task Update(CurrentWar storedWar)
//        {
//            CurrentWar? downloadedWar;

//            if (storedWar is LeagueWar storedLeagueWar)
//            {
//                downloadedWar = await _cocApi.Wars.GetLeagueWarAsync(storedLeagueWar.WarTag, false).ConfigureAwait(false);
//            }
//            else
//            {
//                downloadedWar = await _cocApi.Wars.GetCurrentWarAsync(storedWar).ConfigureAwait(false) as CurrentWar;
//            }

//            if (downloadedWar != null)
//                downloadedWar.Update(_cocApi, storedWar);
//        }
//    }
//}