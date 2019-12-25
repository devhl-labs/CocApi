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
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using devhl.CocApi.Models.Location;
using System.Collections.Immutable;
using System.Collections.Concurrent;

namespace devhl.CocApi
{
    //public delegate Task ApiIsAvailableChangedEventHandler(bool isAvailable);
    //public delegate Task ClanChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    //public delegate Task VillagesJoinedEventHandler(ClanApiModel oldClan, IReadOnlyList<ClanVillageApiModel> villageListApiModels);
    //public delegate Task VillagesLeftEventHandler(ClanApiModel oldClan, IReadOnlyList<ClanVillageApiModel> villageListApiModels);
    //public delegate Task ClanBadgeUrlChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    //public delegate Task ClanLocationChangedEventHandler(ClanApiModel oldClan, ClanApiModel newClan);
    //public delegate Task NewWarEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task WarChangedEventHandler(ICurrentWarApiModel oldWar, ICurrentWarApiModel newWar);
    //public delegate Task NewAttacksEventHandler(ICurrentWarApiModel currentWarApiModel, IReadOnlyList<AttackApiModel> newAttacks);
    //public delegate Task WarEndingSoonEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task WarStartingSoonEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task ClanVersusPointsChangedEventHandler(ClanApiModel oldClan, int newClanVersusPoints);
    //public delegate Task ClanPointsChangedEventHandler(ClanApiModel oldClan, int newClanPoints);
    //public delegate Task WarIsAccessibleChangedEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task WarEndNotSeenEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task VillageChangedEventHandler(VillageApiModel oldVillage, VillageApiModel newVillage);
    //public delegate Task VillageDefenseWinsChangedEventHandler(VillageApiModel oldVillage, int newDefenseWins);
    //public delegate Task VillageExpLevelChangedEventHandler(VillageApiModel oldVillage, int newExpLevel);
    //public delegate Task VillageTrophiesChangedEventHandler(VillageApiModel oldVillage, int newTrophies);
    //public delegate Task VillageVersusBattleWinCountChangedEventHandler(VillageApiModel oldVillage, int newVersusBattleWinCount);
    //public delegate Task VillageVersusBattleWinsChangedEventHandler(VillageApiModel oldVillage, int newVersusBattleWins);
    //public delegate Task VillageVersusTrophiesChangedEventHandler(VillageApiModel oldVillage, int newVersusTrophies);
    //public delegate Task VillageAchievementsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<AchievementApiModel> newAchievements);
    //public delegate Task VillageTroopsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<TroopApiModel> newTroops);
    //public delegate Task VillageHeroesChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<TroopApiModel> newHeroes);
    //public delegate Task VillageSpellsChangedEventHandler(VillageApiModel oldVillage, IReadOnlyList<VillageSpellApiModel> newSpells);
    //public delegate Task WarStartedEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task WarEndedEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task WarEndSeenEventHandler(ICurrentWarApiModel currentWarApiModel);
    //public delegate Task LeagueGroupTeamSizeChangedEventHandler(LeagueGroupApiModel leagueGroupApiModel);
    //public delegate Task ClanLabelsChangedEventHandler(ClanApiModel newClanApiModel, IReadOnlyList<ClanLabelApiModel> addedLabels, IReadOnlyList<ClanLabelApiModel> removedLables);
    //public delegate Task VillageLabelsChangedEventHandler(VillageApiModel newVillageApiModel, IReadOnlyList<VillageLabelApiModel> addedLabels, IReadOnlyList<VillageLabelApiModel> removedLabels);
    //public delegate Task VillageReachedLegendsLeagueEventHandler(VillageApiModel villageApiModel);
    //public delegate Task ClanDonationsEventHandler(ClanApiModel oldClan, IReadOnlyList<Donation> receivedDonations, IReadOnlyList<Donation> gaveDonations);
    //public delegate Task ClanVillageNameChangedEventHandler(ClanVillageApiModel oldVillage, string newName);
    //public delegate Task ClanVillagesLeagueChangedEventHandler(ClanApiModel oldClan, IReadOnlyList<LeagueChange> leagueChanged);
    //public delegate Task ClanVillagesRoleChangedEventHandler(ClanApiModel oldClan, IReadOnlyList<RoleChange> roleChanges);
    //public delegate Task ClanDonationsResetEventHandler(ClanApiModel oldClan, ClanApiModel newClan);


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
        public async Task<ClanApiModel?> GetClanOrDefaultAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VerifyInitialization();

            ClanApiModel? result = null;

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
        public async Task<PaginatedApiModel<ClanApiModel>?> GetClansOrDefaultAsync(string? clanName = null
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

            PaginatedApiModel<ClanApiModel>? result = null;

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

        public async Task<ILeagueGroup?> GetLeagueGroupOrDefaultAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
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
        public async Task<IWar?> GetCurrentWarOrDefaultAsync(string clanTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
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
        public async Task<LeagueWarApiModel?> GetLeagueWarOrDefaultAsync(string warTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            LeagueWarApiModel? result = null;

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
        public async Task<VillageApiModel?> GetVillageOrDefaultAsync(string villageTag, bool allowExpiredItem = false, CancellationToken? cancellationToken = null)
        {
            VillageApiModel? result = null;

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
        public async Task<PaginatedApiModel<WarLogEntryModel>?> GetWarLogOrDefaultAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationToken? cancellationToken = null)
        {
            PaginatedApiModel<WarLogEntryModel>? result = null;

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
        /// Returns the most recent download available for the given war.  Will use both clanTags when the war log is private.  Returns null or the most recent <see cref="ICurrentWarApiModel"/> available.
        /// </summary>
        /// <param name="storedWar"></param>
        /// <returns></returns>
        public async Task<ICurrentWarApiModel?> GetCurrentWarOrDefaultAsync(ICurrentWarApiModel storedWar)
        {
            try
            {
                ICurrentWarApiModel? warByWarId = GetWarByWarIdOrDefault(storedWar.WarId);

                if (warByWarId?.IsExpired() == false) return warByWarId;

                if (warByWarId?.State == WarState.WarEnded) return warByWarId;

                if (warByWarId?.StartTimeUtc > DateTime.UtcNow) return warByWarId;

                IWar? war = null;

                if (storedWar is LeagueWarApiModel leagueWar)
                {
                    return await GetLeagueWarOrDefaultAsync(leagueWar.WarTag, allowExpiredItem: false).ConfigureAwait(false);
                }
                else
                {
                    foreach (var clan in storedWar.Clans)
                    {
                        war = await GetCurrentWarOrDefaultAsync(clan.ClanTag, allowExpiredItem: false).ConfigureAwait(false);

                        if (war is ICurrentWarApiModel currentWar1 && currentWar1?.WarId == storedWar.WarId) return currentWar1;
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
        public async Task<PaginatedApiModel<LabelApiModel>?> GetClanLabelsOrDefaultAsync(CancellationToken? cancellationToken = null)
        {
            PaginatedApiModel<LabelApiModel>? result = null;

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
        public async Task<PaginatedApiModel<LabelApiModel>?> GetVillageLabelsOrDefaultAsync(CancellationToken? cancellationToken = null)
        {
            PaginatedApiModel<LabelApiModel>? result = null;

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
        public async Task<PaginatedApiModel<VillageLeagueApiModel>?> GetVillageLeaguesOrDefaultAsync(CancellationToken? cancellationToken = null)
        {
            PaginatedApiModel<VillageLeagueApiModel>? result = null;

            try
            {
                result = await GetVillageLeaguesAsync(cancellationToken).ConfigureAwait(false);
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
