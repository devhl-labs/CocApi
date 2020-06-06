using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Cache;
using devhl.CocApi.Models.War;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Wars
    {
        public Wars(CocApi cocApi) => CocApi = cocApi;

        public event AsyncEventHandler<ChangedEventArgs<WarLogEntry, CurrentWar>>? AddedToWarLog;

        public event AsyncEventHandler<ChangedEventArgs<CurrentWar, IReadOnlyList<Attack>>>? NewAttacks;

        public event AsyncEventHandler<ChangedEventArgs<CurrentWar>>? NewWar;

        /// <summary>
        /// Fires if the war cannot be found from either clanTags or warTag.  Private war logs can also fire this.
        /// </summary>
        public event AsyncEventHandler<ChangedEventArgs<CurrentWar, bool>>? WarAccessibilityChanged;

        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///     <item><description><see cref="CurrentWar.EndTimeUtc"/></description></item>
        ///     <item><description><see cref="CurrentWar.StartTimeUtc"/></description></item>
        ///     <item><description><see cref="CurrentWar.State"/></description></item>
        ///
        /// </list>
        /// </summary>
        public event AsyncEventHandler<ChangedEventArgs<CurrentWar, CurrentWar>>? WarChanged;

        /// <summary>
        /// Fires when the <see cref="CurrentWar.EndTimeUtc"/> has elapsed.  The Api may or may not show the war end when this event occurs.
        /// </summary>
        public event AsyncEventHandler<ChangedEventArgs<CurrentWar>>? WarEnded;

        public event AsyncEventHandler<ChangedEventArgs<CurrentWar>>? WarEndingSoon;

        /// <summary>
        /// Fires when the war is not accessible and the end time has passed.
        /// This war may still become available if one of the clans does not spin and opens their war log.
        /// </summary>
        public event AsyncEventHandler<ChangedEventArgs<CurrentWar>>? WarEndNotSeen;

        /// <summary>
        /// Fires when the Api shows <see cref="CurrentWar.State"/> is <see cref="Enums.WarState.WarEnded"/>
        /// </summary>
        public event AsyncEventHandler<ChangedEventArgs<CurrentWar>>? WarEndSeen;

        public event AsyncEventHandler<ChangedEventArgs<CurrentWar>>? WarStarted;

        public event AsyncEventHandler<ChangedEventArgs<CurrentWar>>? WarStartingSoon;

        public bool DownloadCurrentWars { get; set; }
        public bool DownloadCwl { get; set; }
        private CocApi CocApi { get; }

        public async Task<IWar?> FetchAsync<T>(string tag, CancellationToken? cancellationToken = null) where T : CurrentWar
        {
            if (!CocApi.TryGetValidTag(tag, out string formattedTag))
                throw new InvalidTagException(tag);

            if (typeof(T) == typeof(LeagueWar))
            {
                string path = LeagueWar.Url(formattedTag);

                if (!(await CocApi.FetchAsync<LeagueWar>(path, cancellationToken).ConfigureAwait(false) is IWar war))
                    return null;

                if (war is LeagueWar leagueWar)
                {
                    leagueWar.WarTag = formattedTag;

                    LeagueGroup? leagueGroup = null;

                    foreach (WarClan warClan in leagueWar.WarClans)
                    {
                        leagueGroup = await CocApi.Wars.GetOrFetchLeagueGroupAsync(warClan.ClanTag, CacheOption.AllowExpiredServerResponse).ConfigureAwait(false) as LeagueGroup;

                        if (leagueGroup != null &&
                            leagueGroup.Clans.Count(c => c.ClanTag == leagueWar.WarClans[0].ClanTag) == 1 &&
                            leagueGroup.Clans.Count(c => c.ClanTag == leagueWar.WarClans[1].ClanTag) == 1)
                            break;

                        leagueGroup = null;
                    }

                    if (leagueGroup == null)
                        throw new CocApiException("The LeagueGroup could not be found.");

                    leagueWar.GroupKey = leagueGroup.GroupKey;

                    await WebResponse.CacheAsync(path, leagueWar.ToJson(), EndPoint.LeagueWar, leagueWar.ServerExpirationUtc).ConfigureAwait(false);
                }

                return war;
            }

            return await CocApi.FetchAsync<CurrentWar>(CurrentWar.Url(formattedTag), cancellationToken).ConfigureAwait(false) as IWar;
        }

        public async Task<ILeagueGroup?> FetchLeagueGroupAsync(string clanTag, CancellationToken? cancellationToken = null)
        {
            if (!CocApi.TryGetValidTag(clanTag, out string formattedTag))
                throw new InvalidTagException(clanTag);

            return await CocApi.FetchAsync<LeagueGroup>(LeagueGroup.Url(formattedTag), cancellationToken).ConfigureAwait(false) as ILeagueGroup;
        }

        public async Task<Paginated<WarLeague>?> FetchWarLeaguesAsync(CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<WarLeague>>(WarLeague.Url(), cancellationToken).ConfigureAwait(false) as Paginated<WarLeague>;

        public async Task<Paginated<WarLogEntry>?> FetchWarLogAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<WarLogEntry>>(WarLogEntry.Url(clanTag, limit, after, before), cancellationToken).ConfigureAwait(false) as Paginated<WarLogEntry>;

        public async Task<CurrentWar?> GetActiveWarAsync(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            CachedWar? cachedWar = await CocApi.SqlWriter.Select<CachedWar>()
                                                        .Where(c => (c.ClanTag == clanTag ||
                                                                    c.OpponentTag == clanTag) &&
                                                                    c.EndTime > DateTime.UtcNow)
                                                        .OrderBy(c => c.PrepStartTime)
                                                        .QueryFirstOrDefaultAsync()
                                                        .ConfigureAwait(false);

            return cachedWar?.ToCurrentWar();
        }

        public async Task<IWar?> GetAsync<T>(string tag) where T : CurrentWar
        {
            if (!CocApi.TryGetValidTag(tag, out string formattedTag))
                throw new InvalidTagException(tag);

            string path = (typeof(T) == typeof(LeagueWar)) ?
                LeagueWar.Url(formattedTag) :
                CurrentWar.Url(formattedTag);

            Cache? cache = await CocApi.SqlWriter.Select<Cache>()
                                                 .Where(c => c.Path == path)
                                                 .QuerySingleOrDefaultAsync()
                                                 .ConfigureAwait(false);

            return cache?.Json.Deserialize<IWar>();
        }

        public async Task<ILeagueGroup?> GetLeagueGroupAsync(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            string path = LeagueGroup.Url(formattedTag);

            Cache? cache = await CocApi.SqlWriter.Select<Cache>()
                                                .Where(c => c.Path == path)
                                                .QuerySingleOrDefaultAsync()
                                                .ConfigureAwait(false);

            if (cache == null)
                return null;

            return cache.Json.Deserialize<ILeagueGroup>();
        }

        public async Task<IWar?> GetOrFetchAsync<T>(string tag, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null) where T : CurrentWar
        {
            if (!CocApi.TryGetValidTag(tag, out string formattedTag))
                throw new InvalidTagException(tag);

            IWar? war = await GetAsync<T>(formattedTag).ConfigureAwait(false);

            if (war == null)
                return await FetchAsync<T>(formattedTag, cancellationToken).ConfigureAwait(false);

            if (cacheOption == CacheOption.AllowAny)
                return war;

            //if (cacheOption == CacheOption.AllowLocallyExpired || war.IsLocallyExpired(GetTimeSpan(war)) == false)
            //    return war;

            if (cacheOption == CacheOption.AllowExpiredServerResponse && war.IsLocallyExpired(GetTimeSpan(war)) == false)
                return war;

            return await FetchAsync<T>(formattedTag, cancellationToken).ConfigureAwait(false) ?? war;
        }

        public async Task<ILeagueGroup?> GetOrFetchLeagueGroupAsync(string clanTag, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            ILeagueGroup? cached = await GetLeagueGroupAsync(formattedTag).ConfigureAwait(false);

            if (cached == null)
                return await FetchLeagueGroupAsync(formattedTag, cancellationToken).ConfigureAwait(false);

            if (cacheOption == CacheOption.AllowAny)
                return cached;

            //if (cacheOption == CacheOption.AllowLocallyExpired || cached.IsLocallyExpired(GetTimeSpan(cached)) == false)
            //    return cached;

            if (cacheOption == CacheOption.AllowExpiredServerResponse && cached.IsLocallyExpired(GetTimeSpan(cached)) == false)
                return cached;

            return await FetchLeagueGroupAsync(formattedTag, cancellationToken).ConfigureAwait(false) ?? cached;
        }

        public async Task<Paginated<WarLogEntry>?> GetOrFetchWarLogAsync(string clanTag, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            Paginated<WarLogEntry>? warLog = await GetWarLogAsync(formattedTag).ConfigureAwait(false);

            if (warLog == null)
                return await FetchWarLogAsync(clanTag).ConfigureAwait(false);

            if (cacheOption == CacheOption.AllowAny)
                return warLog;

            //if (cacheOption == CacheOption.AllowLocallyExpired || warLog.IsLocallyExpired(CocApi.CocApiConfiguration.WarLogTimeToLive) == false)
            //    return warLog;

            if (cacheOption == CacheOption.AllowExpiredServerResponse && warLog.IsLocallyExpired(CocApi.CocApiConfiguration.WarLogTimeToLive) == false)
                return warLog;

            Paginated<WarLogEntry>? result = await FetchWarLogAsync(formattedTag, null, null, null, cancellationToken).ConfigureAwait(false);

            if (result == null || result.Items == null || result.Items.Count() == 0)
                return warLog;

            return result ?? warLog;
        }

        public async Task<Paginated<WarLogEntry>?> GetWarLogAsync(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            string path = WarLogEntry.Url(formattedTag);

            Cache? cache = await CocApi.SqlWriter.Select<Cache>()
                                                .Where(c => c.Path == path)
                                                .QueryFirstOrDefaultAsync()
                                                .ConfigureAwait(false);

            if (cache == null)
                return null;

            Paginated<WarLogEntry>? result = JsonConvert.DeserializeObject<Paginated<WarLogEntry>>(cache.Json);

            if (result == null || result.Items == null || result.Items.Count() == 0)
                return null;

            return result;
        }

        public async Task<List<CurrentWar>> GetWarsAsync(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            var cachedWars = await CocApi.SqlWriter.Select<CachedWar>()
                                                   .Where(c => c.ClanTag == clanTag ||
                                                               c.OpponentTag == clanTag)
                                                   .QueryToListAsync()
                                                   .ConfigureAwait(false);

            List<CurrentWar> results = new List<CurrentWar>();

            foreach (CachedWar cachedWar in cachedWars)
                results.Add(cachedWar.ToCurrentWar());

            return results;
        }

        internal void OnAddedToWarLog(WarLogEntry warLogEntry, CurrentWar storedWar) => AddedToWarLog?.Invoke(this, new ChangedEventArgs<WarLogEntry, CurrentWar>(warLogEntry, storedWar));

        internal void OnNewAttacks(CurrentWar fetched, List<Attack> attacks)
        {
            if (attacks.Count > 0)
            {
                NewAttacks?.Invoke(this, new ChangedEventArgs<CurrentWar, IReadOnlyList<Attack>>(fetched, attacks.ToImmutableArray()));
            }
        }

        internal void OnNewWar(CurrentWar fetched)
            => NewWar?.Invoke(this, new ChangedEventArgs<CurrentWar>(fetched));

        internal void OnWarChanged(CurrentWar fetched, CurrentWar queued)
            => WarChanged?.Invoke(this, new ChangedEventArgs<CurrentWar, CurrentWar>(fetched, queued));

        internal void OnWarEnded(CurrentWar fetched)
            => WarEnded?.Invoke(this, new ChangedEventArgs<CurrentWar>(fetched));

        internal void OnWarEndingSoon(CurrentWar fetched)
            => WarEndingSoon?.Invoke(this, new ChangedEventArgs<CurrentWar>(fetched));

        internal void OnWarEndNotSeen(CurrentWar fetched)
            => WarEndNotSeen?.Invoke(this, new ChangedEventArgs<CurrentWar>(fetched));

        internal void OnWarEndSeen(CurrentWar currentWar)
            => WarEndSeen?.Invoke(this, new ChangedEventArgs<CurrentWar>(currentWar));

        internal void OnWarIsAccessibleChanged(CurrentWar currentWar, bool isAccessible)
            => WarAccessibilityChanged?.Invoke(this, new ChangedEventArgs<CurrentWar, bool>(currentWar, isAccessible));

        internal void OnWarStarted(CurrentWar fetched)
            => WarStarted?.Invoke(this, new ChangedEventArgs<CurrentWar>(fetched));

        internal void OnWarStartingSoon(CurrentWar fetched)
            => WarStartingSoon?.Invoke(this, new ChangedEventArgs<CurrentWar>(fetched));

        private TimeSpan GetTimeSpan(IWar war)
        {
            if (war is NotInWar)
                return CocApi.CocApiConfiguration.NotInWarTimeToLive;

            if (war is PrivateWarLog)
                return CocApi.CocApiConfiguration.PrivateWarLogTimeToLive;

            if (war is CurrentWar currentWar && currentWar.State == WarState.Preparation)
                return currentWar.StartTimeUtc - DateTime.UtcNow;

            if (war is LeagueWar)
                return CocApi.CocApiConfiguration.LeagueWarTimeToLive;

            if (war is CurrentWar)
                return CocApi.CocApiConfiguration.CurrentWarTimeToLive;

            if (war is WarLogEntry)
                return CocApi.CocApiConfiguration.WarLogTimeToLive;

            throw new CocApiException("Unhandled type");
        }

        private TimeSpan GetTimeSpan(ILeagueGroup iLeagueGroup)
        {
            if (iLeagueGroup is LeagueGroupNotFound)
                return CocApi.CocApiConfiguration.LeagueGroupNotFoundTimeToLive;

            LeagueGroup leagueGroup = (LeagueGroup)iLeagueGroup;

            if (leagueGroup.State == LeagueState.WarsEnded)
                return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1)) - DateTime.UtcNow;

            return CocApi.CocApiConfiguration.LeagueGroupTimeToLive;
        }
    }
}