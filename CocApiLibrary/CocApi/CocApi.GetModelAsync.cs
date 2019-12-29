using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

using devhl.CocApi.Models;
using devhl.CocApi.Exceptions;
using static devhl.CocApi.Enums;
using static devhl.CocApi.Extensions;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System.Collections.Immutable;
using System.Collections.Concurrent;

namespace devhl.CocApi
{
    public sealed partial class CocApi : IDisposable
    {
        public async Task<Clan> GetClanAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();
            
            try
            {
                ThrowIfInvalidTag(clanTag);

                Clan? clan = GetClanOrDefault(clanTag);

                if (clan != null && (allowExpiredItem || clan.IsExpired() == false)) return clan;                    

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}";

                clan = (Clan) await GetAsync<Clan>(url, EndPoint.Clan, cancellationToken).ConfigureAwait(false);

                if (!CocApiConfiguration.CacheHttpResponses) return clan;
                
                lock (AllClans)
                {
                    AllClans[clan.ClanTag] = clan;
                }                

                return clan;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<IWar> GetCurrentWarAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {         
            VerifyInitialization();

            try
            {
                ThrowIfInvalidTag(clanTag);

                IWar? war = GetWarByClanTagOrDefault(clanTag);

                if (war != null && (allowExpiredItem || !war.IsExpired())) return war;

                if (war is IActiveWar currentWar && currentWar.StartTimeUtc > DateTime.UtcNow) return currentWar;

                string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar";

                IDownloadable downloadable = await GetAsync<CurrentWar>(url, EndPoint.CurrentWar, cancellationToken).ConfigureAwait(false);

                if (downloadable is NotInWar notInWar)
                {
                    if (CocApiConfiguration.CacheHttpResponses)
                    {
                        lock (AllWarsByClanTag)
                        {
                            AllWarsByClanTag[clanTag] = notInWar;
                        }
                    }

                    return notInWar;
                }

                IActiveWar downloadedWar = (IActiveWar) downloadable;

                if (!CocApiConfiguration.CacheHttpResponses) return downloadedWar;
                
                foreach(var clan in downloadedWar.Clans)
                {
                    AllWarsByClanTag.TryGetValue(clan.ClanTag, out IWar storedWar, AllWarsByClanTag);

                    if (storedWar == null || storedWar.CacheExpiresAtUtc < downloadedWar.CacheExpiresAtUtc)
                    {
                        lock (AllWarsByClanTag)
                        {
                            AllWarsByClanTag[clan.ClanTag] = downloadedWar;
                        }
                    }

                    if (AllClans.TryGetValue(clan.ClanTag, out Clan storedClan, AllClans))
                    {
                        storedClan.Wars.TryAdd(downloadedWar.WarId, (CurrentWar) downloadedWar, storedClan.Wars);
                    }
                }

                lock (AllWarsByWarId)
                {
                    AllWarsByWarId[downloadedWar.WarId] = downloadedWar;
                }                                                        

                return downloadedWar;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

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
        public async Task<ILeagueGroup> GetLeagueGroupAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanTag)}/currentwar/leaguegroup";

            try
            {
                ThrowIfInvalidTag(clanTag);

                ILeagueGroup? leagueGroup = GetLeagueGroupOrDefault(clanTag);

                if (leagueGroup != null && (allowExpiredItem || !leagueGroup.IsExpired())) return leagueGroup;
                
                IDownloadable downloadable = await GetAsync<LeagueGroup>(url, EndPoint.LeagueGroup, cancellationToken).ConfigureAwait(false);

                if (downloadable is LeagueGroupNotFound notFound)
                {
                    lock (AllLeagueGroups)
                    {
                        AllLeagueGroups[clanTag] = notFound;
                    }

                    return notFound;
                }

                if (!(downloadable is LeagueGroup leagueGroupApiModel)) throw new CocApiException("Unknown Type");

                if (!CocApiConfiguration.CacheHttpResponses) return leagueGroupApiModel;

                foreach(var clan in leagueGroupApiModel.Clans.EmptyIfNull())
                {
                    if (AllLeagueGroups.TryAdd(clan.ClanTag, leagueGroupApiModel, AllLeagueGroups)) continue;

                    if (!AllLeagueGroups.TryGetValue(clan.ClanTag, out ILeagueGroup storedLeagueGroup, AllLeagueGroups)) continue;

                    //the league group already exists.  Lets check if the existing one is from last month
                    if (storedLeagueGroup is LeagueGroup storedLeagueGroupApiModel && leagueGroupApiModel.Season > storedLeagueGroupApiModel.Season)
                    {
                        lock (AllLeagueGroups)
                        {
                            AllLeagueGroups[clan.ClanTag] = storedLeagueGroupApiModel;
                        }
                    }
                }

                return leagueGroupApiModel;
            }

            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<LeagueWar> GetLeagueWarAsync(string warTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

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

                LeagueWar leagueWarApiModel = (LeagueWar) await GetAsync<LeagueWar>(url, EndPoint.LeagueWar, cancellationToken).ConfigureAwait(false);

                leagueWarApiModel.WarTag = warTag;

                leagueWarApiModel.WarType = WarType.SCCWL;

                if (!CocApiConfiguration.CacheHttpResponses) return leagueWarApiModel;

                lock (AllWarsByWarTag)
                {
                    AllWarsByWarTag[leagueWarApiModel.WarTag] = leagueWarApiModel;
                }

                lock (AllWarsByWarId)
                {
                    AllWarsByWarId[leagueWarApiModel.WarId] = leagueWarApiModel;
                }

                foreach(var clan in leagueWarApiModel.Clans)
                {
                    if (AllClans.TryGetValue(clan.ClanTag, out Clan storedClan, AllClans))
                    {
                        storedClan.Wars.TryAdd(leagueWarApiModel.WarId, leagueWarApiModel, storedClan.Wars);
                    }
                }

                foreach(var clan in leagueWarApiModel.Clans)
                {
                    if (AllWarsByClanTag.TryGetValue(clan.ClanTag, out IWar war, AllWarsByClanTag))
                    {
                        if (war is NotInWar || leagueWarApiModel.State == WarState.InWar)
                        {
                            lock (AllWarsByClanTag)
                            {
                                AllWarsByClanTag[clan.ClanTag] = leagueWarApiModel;
                            }
                        }
                        else if (war is IActiveWar currentWar && (DateTime.UtcNow > currentWar.EndTimeUtc && DateTime.UtcNow < leagueWarApiModel.EndTimeUtc))
                        {
                            lock (AllWarsByClanTag)
                            {
                                AllWarsByClanTag[clan.ClanTag] = leagueWarApiModel;
                            }
                        }
                    }
                    else
                    {
                        lock (AllWarsByClanTag)
                        {
                            AllWarsByClanTag[clan.ClanTag] = leagueWarApiModel;
                        }
                    }
                }

                return leagueWarApiModel;
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }

        public async Task<Village> GetVillageAsync(string villageTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                ThrowIfInvalidTag(villageTag);

                Village? villageApiModel = GetVillageOrDefault(villageTag);

                if (villageApiModel != null && (allowExpiredItem || !villageApiModel.IsExpired())) return villageApiModel;

                string url = $"https://api.clashofclans.com/v1/players/{Uri.EscapeDataString(villageTag)}";

                villageApiModel = (Village) await GetAsync<Village>(url, EndPoint.Village, cancellationToken).ConfigureAwait(false);

                if (CocApiConfiguration.CacheHttpResponses)
                {
                    lock (AllVillages)
                    {
                        AllVillages[villageTag] = villageApiModel;
                    }
                }

                return villageApiModel;
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
            VerifyInitialization();

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
            VerifyInitialization();

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
        public async Task<Paginated<VillageLeague>> GetVillageLeaguesAsync(CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/leagues?limit=500";

                return (Paginated<VillageLeague>) await GetAsync<Paginated<VillageLeague>>(url, EndPoint.VillageLeagues, cancellationToken).ConfigureAwait(false);
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
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations?limit=10000";

                return (Paginated<Location>) await GetAsync<Paginated<Location>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);
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
        public async Task<Paginated<TopMainClan>> GetTopMainClansAsync(int locationId, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations/{locationId}/rankings/clans";

                return (Paginated<TopMainClan>) await GetAsync<Paginated<TopMainClan>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);  //todo why is the end point locations
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

        public async Task<Paginated<TopBuilderClan>> GetTopBuilderClansAsync(int locationId, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations/{locationId}/rankings/clans-versus";

                return (Paginated<TopBuilderClan>) await GetAsync<Paginated<TopBuilderClan>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false);  //todo why is the end point locations
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

        public async Task<Paginated<TopMainVillage>> GetTopMainVillagesAsync(int locationId, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations/{locationId}/rankings/players";

                return (Paginated<TopMainVillage>) await GetAsync<Paginated<TopMainVillage>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false); //todo why is the end point locations
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

        public async Task<Paginated<TopBuilderVillage>> GetTopBuilderVillagesAsync(int locationId, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/locations/{locationId}/rankings/players-versus";

                return (Paginated<TopBuilderVillage>) await GetAsync<Paginated<TopBuilderVillage>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false); //todo why is the end point locations
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
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/labels/clans?limit=10000";

                return (Paginated<Label>) await GetAsync<Paginated<Label>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false); //todo why is the end point locations
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
            VerifyInitialization();

            try
            {
                string url = $"https://api.clashofclans.com/v1/labels/players?limit=10000";

                return (Paginated<Label>) await GetAsync<Paginated<Label>>(url, EndPoint.Locations, cancellationToken).ConfigureAwait(false); //todo why is the end point locations
            }
            catch (Exception e)
            {
                if (e is CocApiException) throw;

                throw new CocApiException(e.Message, e);
            }
        }
    }
}
