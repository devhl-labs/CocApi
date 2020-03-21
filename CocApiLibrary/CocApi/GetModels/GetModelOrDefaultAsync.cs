using System;
using System.Threading.Tasks;
using System.Threading;

using devhl.CocApi.Models;
using devhl.CocApi.Exceptions;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;

namespace devhl.CocApi
{
    public sealed partial class CocApi : IDisposable
    {

        /// <summary>
        /// Returns null if the clanTag is not found.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Clan?> GetClanOrDefaultAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            ThrowIfNotInitialized();

            Clan? result = null;

            try
            {
                result = await GetClanAsync(clanTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// This will not throw a <see cref="ServerResponseException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Paginated<Clan>?> GetClansOrDefaultAsync(string? clanName = null
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

            Paginated<Clan>? result = null;

            try
            {
                result = await GetClansAsync(clanName, warFrequency, locationId, minVillages, maxVillages, minClanPoints, minClanLevel, limit, after, before, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            //catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the clan is not in a league.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>

        public async Task<ILeagueGroup?> GetLeagueGroupOrDefaultAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            ILeagueGroup? result = null;

            try
            {
                result = await GetLeagueGroupAsync(clanTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// Returns null if the war log is private.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<IWar?> GetCurrentWarOrDefaultAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            IWar? result = null;

            try
            {
                result = await GetCurrentWarAsync(clanTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the warTag is not found.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="warTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<LeagueWar?> GetLeagueWarOrDefaultAsync(string warTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            LeagueWar? result = null;

            try
            {
                result = await GetLeagueWarAsync(warTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the villageTag is not found.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="villageTag"></param>
        /// <param name="allowStoredItem"></param>
        /// <param name="allowExpiredItem"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Village?> GetVillageOrDefaultAsync(string villageTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            Village? result = null;

            try
            {
                result = await GetVillageAsync(villageTag, allowExpiredItem, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        /// <summary>
        /// Returns null if the clan is not in a league.  This will not throw a <see cref="ServerResponseException"/> nor a <see cref="InvalidTagException"/>.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <param name="limit"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Paginated<WarLogEntry>?> GetWarLogOrDefaultAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationToken? cancellationToken = null)
        {
            Paginated<WarLogEntry>? result = null;

            try
            {
                result = await GetWarLogAsync(clanTag, limit, after, before, cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (InvalidTagException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// Returns the most recent download available for the given war.  Will use both clanTags when the war log is private.  Returns null or the most recent <see cref="IActiveWar"/> available.
        /// </summary>
        /// <param name="storedWar"></param>
        /// <returns></returns>
        public async Task<CurrentWar?> GetCurrentWarOrDefaultAsync(CurrentWar storedWar)
        {
            try
            {
                CurrentWar? warByWarId = GetWarByWarIdOrDefault(storedWar.WarKey);

                if (warByWarId?.IsExpired() == false) return warByWarId;

                if (warByWarId?.State == WarState.WarEnded) return warByWarId;

                if (warByWarId?.StartTimeUtc > DateTime.UtcNow) return warByWarId;

                IWar? war = null;

                if (storedWar is LeagueWar leagueWar)
                {
                    return await GetLeagueWarOrDefaultAsync(leagueWar.WarTag, allowExpiredItem: false).ConfigureAwait(false);
                }
                else
                {
                    foreach (var clan in storedWar.Clans)
                    {
                        war = await GetCurrentWarOrDefaultAsync(clan.ClanTag, allowExpiredItem: false).ConfigureAwait(false);

                        if (war is CurrentWar currentWar && currentWar?.WarKey == storedWar.WarKey) return currentWar;
                    }
                }

                return null;
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
        public async Task<Paginated<Label>?> GetClanLabelsOrDefaultAsync(CancellationToken? cancellationToken = null)
        {
            Paginated<Label>? result = null;

            try
            {
                return await GetClanLabelsAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Paginated<Label>?> GetVillageLabelsOrDefaultAsync(CancellationToken? cancellationToken = null)
        {
            Paginated<Label>? result = null;

            try
            {
                result = await GetVillageLabelsAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ServerResponseException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// This does not cache responses.  Every request will poll the Api.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<Paginated<League>?> GetLeaguesOrDefaultAsync(CancellationToken? cancellationToken = null)
        {
            Paginated<League>? result = null;

            try
            {
                AllLeagues = await GetLeaguesAsync(cancellationToken).ConfigureAwait(false);

                return AllLeagues;
            }
            catch (ServerResponseException) { }
            catch (Exception)
            {
                throw;
            }

            return result;
        }
    }
}
