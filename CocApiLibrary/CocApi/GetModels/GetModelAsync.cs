using System;
using System.Threading.Tasks;
using System.Threading;

using devhl.CocApi.Models;
using devhl.CocApi.Exceptions;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System.Linq;

namespace devhl.CocApi
{
    public sealed partial class CocApi : IDisposable
    {
        public async Task<Clan> GetClanAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();
            
            try
            {
                ThrowIfInvalidTag(clanTag);

                Clan? clan = GetClanOrDefault(clanTag);

                if (clan != null && (allowExpiredItem || clan.IsExpired() == false)) return clan;                    

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}";

                clan = (Clan) await GetAsync<Clan>(url, EndPoint.Clan, cancellationToken).ConfigureAwait(false);

                if (!CocApiConfiguration.CacheHttpResponses) return clan;

                AllClans.AddOrUpdate(clan.ClanTag, clan, (clanTag, clan2) =>
                {
                    if (clan.DownloadedAtUtc > clan2.DownloadedAtUtc) return clan;

                    return clan2;
                });                      

                return clan;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        //public async Task<IWar> GetCurrentWarAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        //{         
        //    ThrowIfNotInitialized();

        //    try
        //    {
        //        ThrowIfInvalidTag(clanTag);

        //        IWar? war = GetCurrentWarByClanTagOrDefault(clanTag);

        //        if (war != null && (allowExpiredItem || !war.IsExpired())) return war;

        //        if (war is CurrentWar currentWar && currentWar.StartTimeUtc > DateTime.UtcNow) return currentWar;

        //        string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar";

        //        Downloadable downloadable = await GetAsync<CurrentWar>(url, EndPoint.CurrentWar, cancellationToken).ConfigureAwait(false);

        //        if (downloadable is NotInWar notInWar)
        //        {
        //            if (CocApiConfiguration.CacheHttpResponses)
        //            {
        //                if (AllCurrentWarsByClanTag.TryGetValue(clanTag, out IWar storedWar) == false ||
        //                    storedWar is NotInWar ||
        //                    (storedWar is LeagueWar leagueWar && leagueWar.State == WarState.WarEnded))
        //                {
        //                    AllCurrentWarsByClanTag.AddOrUpdate(clanTag, notInWar, (clanTag, war2) =>
        //                    {
        //                        if (notInWar.DownloadedAtUtc > war2.DownloadedAtUtc) return notInWar;

        //                        return war2;
        //                    });
        //                }
        //            }

        //            return notInWar;
        //        }

        //        if (downloadable is PrivateWarLog privateWarLog)
        //        {
        //            if (CocApiConfiguration.CacheHttpResponses)
        //            {
        //                AllCurrentWarsByClanTag.AddOrUpdate(clanTag, privateWarLog, (clanTag, war2) =>
        //                {
        //                    if (privateWarLog.DownloadedAtUtc > war2.DownloadedAtUtc) return privateWarLog;

        //                    return war2;
        //                });
        //            }

        //            return privateWarLog;
        //        }

        //        CurrentWar downloadedWar = (CurrentWar) downloadable;

        //        if (!CocApiConfiguration.CacheHttpResponses) return downloadedWar;

        //        foreach(var clan in downloadedWar.WarClans)
        //        {
        //            AllCurrentWarsByClanTag.TryGetValue(clan.ClanTag, out IWar storedWar);

        //            if (storedWar == null || storedWar.ServerResponseRefreshesAtUtc < downloadedWar.ServerResponseRefreshesAtUtc || storedWar is PrivateWarLog)
        //            {
        //                AllCurrentWarsByClanTag.AddOrUpdate(clan.ClanTag, downloadedWar, (clanTag, war2) =>
        //                {
        //                    if (downloadedWar.DownloadedAtUtc > war2.DownloadedAtUtc) return downloadedWar;

        //                    return war2;
        //                });
        //            }

        //            if (AllClans.TryGetValue(clan.ClanTag, out Clan storedClan))
        //            {
        //                storedClan.Wars.TryAdd(downloadedWar.WarKey, (CurrentWar) downloadedWar);
        //            }
        //        }             

        //        AllWarsByWarKey.AddOrUpdate(downloadedWar.WarKey, downloadedWar, (_, war2) =>
        //        {
        //            if (downloadedWar.DownloadedAtUtc > war2.DownloadedAtUtc) return downloadedWar;

        //            return war2;
        //        });

        //        return downloadedWar;
        //    }
        //    catch (Exception e)
        //    {
        //        if (e is CocApiException) throw;

        //        throw new CocApiException(e.Message, e);
        //    }
        //}

        /// <summary>
        /// Returns <see cref="NotInWar"/>, <see cref="PrivateWarLog"/>, <see cref="CurrentWar"/>, or <see cref="LeagueWar"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IWar> GetCurrentWarAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            try
            {
                ThrowIfInvalidTag(clanTag);

                IWar? war = GetCurrentWarByClanTagOrDefault(clanTag);

                if (war == null || war is NotInWar || war is PrivateWarLog)
                {
                    if (AllLeagueGroups.TryGetValue(clanTag, out ILeagueGroup iLeagueGroup) && iLeagueGroup is LeagueGroup leagueGroup)
                    {
                        LeagueWar? leagueWarInPrep = null;

                        foreach(Round round in leagueGroup.Rounds.EmptyIfNull())
                        {
                            foreach (string warTag in round.WarTags.Where(wt => wt != "#0"))
                            {
                                LeagueWar? leagueWar = GetLeagueWarOrDefault(warTag);

                                if (leagueWar == null || !leagueWar.WarClans.Any(wc => wc.ClanTag == clanTag))
                                    continue;

                                if (leagueWar.State == WarState.InWar)
                                    return leagueWar;

                                if (leagueWar.State == WarState.Preparation)
                                    leagueWarInPrep = leagueWar;
                            }
                        }

                        if (leagueWarInPrep != null)
                            return leagueWarInPrep;
                    }
                }

                if (war != null && (allowExpiredItem || !war.IsExpired()))
                    return war;

                if (war is CurrentWar storedWar && storedWar.StartTimeUtc > DateTime.UtcNow)
                    return storedWar;

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar";

                war = (IWar) await GetAsync<CurrentWar>(url, EndPoint.CurrentWar, cancellationToken).ConfigureAwait(false);

                if (!CocApiConfiguration.CacheHttpResponses)
                    return war;

                if (war is NotInWar || war is PrivateWarLog)
                {
                    AllCurrentWarsByClanTag.AddOrUpdate(clanTag, war, (_, war2) =>
                    {
                        if (war.DownloadedAtUtc > war2.DownloadedAtUtc)
                            return war;

                        return war2;
                    });

                    return war;
                }

                CurrentWar currentWar = (CurrentWar)war;

                foreach (var clan in currentWar.WarClans)
                {
                    AllCurrentWarsByClanTag.AddOrUpdate(clan.ClanTag, currentWar, (_, war2) =>
                    {
                        if (currentWar.DownloadedAtUtc > war2.DownloadedAtUtc)
                            return currentWar;

                        return war2;
                    });

                    if (AllClans.TryGetValue(clan.ClanTag, out Clan storedClan))
                    {
                        storedClan.Wars.TryAdd(currentWar.WarKey, currentWar);
                    }
                }

                AllWarsByWarKey.AddOrUpdate(currentWar.WarKey, currentWar, (_, war2) =>
                {
                    if (currentWar.DownloadedAtUtc > war2.DownloadedAtUtc)
                        return currentWar;

                    return war2;
                });

                return currentWar;
            }
            catch (Exception e)
            {
                if (e is CocApiException)
                    throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// Returns <see cref="LeagueGroup"/> or <see cref="LeagueGroupNotFound"/>
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<ILeagueGroup> GetLeagueGroupAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar/leaguegroup";

            try
            {
                ThrowIfInvalidTag(clanTag);

                ILeagueGroup? iLeagueGroup = GetLeagueGroupOrDefault(clanTag);

                if (iLeagueGroup != null && (allowExpiredItem || !iLeagueGroup.IsExpired())) return iLeagueGroup;
                
                iLeagueGroup = (ILeagueGroup) await GetAsync<LeagueGroup>(url, EndPoint.LeagueGroup, cancellationToken).ConfigureAwait(false);

                if (!CocApiConfiguration.CacheHttpResponses) return iLeagueGroup;

                if (iLeagueGroup is LeagueGroupNotFound notFound)
                {
                    AllLeagueGroups.AddOrUpdate(clanTag, notFound, (_, group2) =>
                    {
                        if (notFound.DownloadedAtUtc > group2.DownloadedAtUtc) return notFound;
                        
                        return group2;
                    });

                    return notFound;
                }

                LeagueGroup leagueGroup = (LeagueGroup)iLeagueGroup;              

                foreach(var clan in leagueGroup.Clans.EmptyIfNull())
                {
                    AllLeagueGroups.AddOrUpdate(clan.ClanTag, leagueGroup, (_, iLeagueGroup2) =>
                    {
                        if (iLeagueGroup2 is LeagueGroup leagueGroup2 && leagueGroup.Season != leagueGroup2.Season)
                        {
                            if (leagueGroup.Season > leagueGroup2.Season)
                                return leagueGroup;

                            return leagueGroup2;
                        }

                        if (leagueGroup.DownloadedAtUtc > iLeagueGroup2.DownloadedAtUtc)
                            return leagueGroup;

                        return iLeagueGroup2;
                    });
                }

                return leagueGroup;
            }

            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<LeagueWar> GetLeagueWarAsync(string warTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            try
            {
                ThrowIfInvalidTag(warTag);

                LeagueWar? leagueWar = GetLeagueWarOrDefault(warTag);

                if (leagueWar != null)
                {
                    if (allowExpiredItem || !leagueWar.IsExpired()) return leagueWar;

                    if (leagueWar.State == WarState.WarEnded) return leagueWar;

                    if (leagueWar.StartTimeUtc > DateTime.UtcNow) return leagueWar;
                }

                string url = $"https://api.clashofclans.com/v1/clanwarleagues/wars/{Uri.EscapeDataString(warTag)}";

                leagueWar = (LeagueWar) await GetAsync<LeagueWar>(url, EndPoint.LeagueWar, cancellationToken).ConfigureAwait(false);

                leagueWar.WarTag = warTag;

                if (!CocApiConfiguration.CacheHttpResponses) return leagueWar;
                
                AllLeagueWarsByWarTag.AddOrUpdate(leagueWar.WarTag, leagueWar, (_, leagueWar2) =>
                {
                    if (leagueWar.DownloadedAtUtc > leagueWar2.DownloadedAtUtc) return leagueWar;

                    return leagueWar2;
                });

                AllWarsByWarKey.AddOrUpdate(leagueWar.WarKey, leagueWar, (_, leagueWar2) =>
                {
                    if (leagueWar.DownloadedAtUtc > leagueWar2.DownloadedAtUtc) return leagueWar;

                    return leagueWar2;
                });
                
                foreach(var clan in leagueWar.WarClans)
                {
                    if (AllClans.TryGetValue(clan.ClanTag, out Clan storedClan))
                    {
                        storedClan.Wars.TryAdd(leagueWar.WarKey, leagueWar);
                    }
                }

                return leagueWar;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<Village> GetVillageAsync(string villageTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            try
            {
                ThrowIfInvalidTag(villageTag);

                Village? village = GetVillageOrDefault(villageTag);

                if (village != null && (allowExpiredItem || !village.IsExpired())) return village;

                string url = $"https://api.clashofclans.com/v1/players/{Uri.EscapeDataString(villageTag)}";

                village = (Village) await GetAsync<Village>(url, EndPoint.Village, cancellationToken).ConfigureAwait(false);

                if (!CocApiConfiguration.CacheHttpResponses)
                    return village;
                
                AllVillages.AddOrUpdate(villageTag, village, (_, village2) =>
                {
                    if (village.DownloadedAtUtc > village2.DownloadedAtUtc) return village;

                    return village2;
                });

                return village;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Paginated<WarLogEntry>> GetWarLogAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            try
            {
                ThrowIfInvalidTag(clanTag);

                string url = $"https://api.clashofclans.com/v1/clans/";

                url = $"{url}{Uri.EscapeDataString(clanTag)}/warlog?";

                if (limit != null)
                {
                    url = $"{url}limit={limit}&";
                }

                if (after != null)
                {
                    url = $"{url}after={after}&";
                }

                if (before != null)
                {
                    url = $"{url}before={before}&";
                }

                if (url.EndsWith("&"))
                {
                    url = url[0..^1];
                }

                if (url.EndsWith("?"))
                {
                    url = url[0..^1];
                }

                return (Paginated<WarLogEntry>) await GetAsync<Paginated<WarLogEntry>>(url, EndPoint.WarLog, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Paginated<Clan>> GetClansAsync(string? clanName = null
                                                        , WarFrequency? warFrequency = null
                                                        , int? locationId = null
                                                        , int? minVillages = null
                                                        , int? maxVillages = null
                                                        , int? minClanPoints = null
                                                        , int? minClanLevel = null
                                                        , int? limit = null
                                                        , int? after = null
                                                        , int? before = null
                                                        , CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            try
            {
                if (!string.IsNullOrEmpty(clanName) && clanName.Length < 3)
                {
                    throw new ArgumentException("The clan name must be longer than three characters.");
                }

                string url = $"https://api.clashofclans.com/v1/clans?";

                if (clanName != null)
                {
                    url = $"{url}name={Uri.EscapeDataString(clanName)}&";
                }
                if (warFrequency != null)
                {
                    url = $"{url}warFrequency={warFrequency.ToString()}&";
                }
                if (locationId != null)
                {
                    url = $"{url}locationId={locationId}&";
                }
                if (minVillages != null)
                {
                    url = $"{url}minMembers={minVillages}&";
                }
                if (maxVillages != null)
                {
                    url = $"{url}maxMembers={maxVillages}&";
                }
                if (minClanPoints != null)
                {
                    url = $"{url}minClanPoints={minClanPoints}&";
                }
                if (minClanLevel != null)
                {
                    url = $"{url}minClanLevel={minClanLevel}&";
                }
                if (limit != null)
                {
                    url = $"{url}limit={limit}&";
                }
                if (after != null)
                {
                    url = $"{url}after={after}&";
                }
                if (before != null)
                {
                    url = $"{url}before={before}&";
                }

                if (url.EndsWith("&"))
                {
                    url = url[0..^1];
                }

                return (Paginated<Clan>) await GetAsync<Paginated<Clan>>(url, EndPoint.Clans, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Paginated<League>> GetLeaguesAsync(CancellationToken? cancellationToken = null)
        {
            //ThrowIfNotInitialized();

            try
            {
                string url = $"https://api.clashofclans.com/v1/leagues?limit=500";

                AllLeagues = (Paginated<League>) await GetAsync<Paginated<League>>(url, EndPoint.VillageLeagues, cancellationToken).ConfigureAwait(false);

                return AllLeagues;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Paginated<Location>> GetLocationsAsync(CancellationToken? cancellationToken = null)
        {
            //ThrowIfNotInitialized();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations?limit=10000";

                AllLocations = (Paginated<Location>) await GetAsync<Paginated<Location>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);

                return AllLocations;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }        
        
        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Paginated<TopMainClan>> GetTopMainClansAsync(int? locationId = null, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            try
            {
                string location = "global";

                if (locationId != null) location = locationId.ToString();

                string url = $"https://api.clashofclans.com/v1/locations/{location}/rankings/clans";

                return (Paginated<TopMainClan>) await GetAsync<Paginated<TopMainClan>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<Paginated<TopBuilderClan>> GetTopBuilderClansAsync(int? locationId = null, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            try
            {
                string location = "global";

                if (locationId != null) location = locationId.ToString();

                string url = $"https://api.clashofclans.com/v1/locations/{location}/rankings/clans-versus";

                return (Paginated<TopBuilderClan>) await GetAsync<Paginated<TopBuilderClan>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }
        
        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<Paginated<TopMainVillage>> GetTopMainVillagesAsync(int? locationId = null, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            try
            {
                string location = "global";

                if (locationId != null) location = locationId.ToString();

                string url = $"https://api.clashofclans.com/v1/locations/{location}/rankings/players";

                return (Paginated<TopMainVillage>) await GetAsync<Paginated<TopMainVillage>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<Paginated<TopBuilderVillage>> GetTopBuilderVillagesAsync(int? locationId = null, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            try
            {
                string location = "global";

                if (locationId != null) location = locationId.ToString();

                string url = $"https://api.clashofclans.com/v1/locations/{location}/rankings/players-versus";

                return (Paginated<TopBuilderVillage>) await GetAsync<Paginated<TopBuilderVillage>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<Paginated<Label>> GetClanLabelsAsync(CancellationToken? cancellationToken = null)
        {
            //ThrowIfNotInitialized();

            try
            {
                string url = $"https://api.clashofclans.com/v1/labels/clans?limit=10000";

                AllClanLabels = (Paginated<Label>) await GetAsync<Paginated<Label>>(url, EndPoint.Labels, cancellationToken).ConfigureAwait(false);

                return AllClanLabels;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<Paginated<Label>> GetVillageLabelsAsync(CancellationToken? cancellationToken = null)
        {
            //ThrowIfNotInitialized();

            try
            {
                string url = $"https://api.clashofclans.com/v1/labels/players?limit=10000";

                AllVillageLabels = (Paginated<Label>) await GetAsync<Paginated<Label>>(url, EndPoint.Labels, cancellationToken).ConfigureAwait(false);

                return AllVillageLabels;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }
    }
}
