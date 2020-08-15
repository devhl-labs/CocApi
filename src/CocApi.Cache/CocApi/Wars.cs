using CocApi.Cache.Exceptions;
using CocApi.Cache.Models;
using CocApi.Cache.Models.Cache;
using CocApi.Cache.Models.Wars;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Cache
{
    public sealed class Wars
    {
        public Wars(CocApiClient_old cocApi) => CocApi = cocApi;

        public event AsyncEventHandler<ChangedEventArgs<WarLogEntry, CurrentWar>>? AddedToWarLog;

        public event AsyncEventHandler<ChangedEventArgs<CurrentWar, ImmutableArray<Attack>>>? NewAttacks;

        public event AsyncEventHandler<EventArgs<CurrentWar>>? NewWar;

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
        public event AsyncEventHandler<EventArgs<CurrentWar>>? WarEnded;

        public event AsyncEventHandler<EventArgs<CurrentWar>>? WarEndingSoon;

        /// <summary>
        /// Fires when the war is not accessible and the end time has passed.
        /// This war may still become available if one of the clans does not spin and opens their war log.
        /// </summary>
        public event AsyncEventHandler<EventArgs<CurrentWar>>? WarEndNotSeen;

        /// <summary>
        /// Fires when the Api shows <see cref="CurrentWar.State"/> is <see cref="Enums.WarState.WarEnded"/>
        /// </summary>
        public event AsyncEventHandler<EventArgs<CurrentWar>>? WarEndSeen;

        public event AsyncEventHandler<EventArgs<CurrentWar>>? WarStarted;

        public event AsyncEventHandler<EventArgs<CurrentWar>>? WarStartingSoon;

        public bool DownloadCurrentWars { get; set; }
        public bool DownloadCwl { get; set; }
        private CocApiClient_old CocApi { get; }

        public async Task<IWar?> GetAsync<T>(string tag, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
        {
            if (typeof(T) == typeof(LeagueWar))
                return await CocApi.GetOrFetchAsync<IWar>(LeagueWar.Url(tag), cacheOption, cancellationToken);

            return await CocApi.GetOrFetchAsync<IWar>(CurrentWar.Url(tag), cacheOption, cancellationToken);
        }

        internal async Task<string> GetWarKeyAsync(LeagueWar leagueWar)
        {
            LeagueGroup? leagueGroup = null;

            foreach (WarClan warClan in leagueWar.WarClans)
            {
                leagueGroup = await GetLeagueGroupAsync(warClan.ClanTag).ConfigureAwait(false) as LeagueGroup;

                if (leagueGroup != null &&
                    leagueGroup.Clans.Count(c => c.ClanTag == leagueWar.WarClans[0].ClanTag) == 1 &&
                    leagueGroup.Clans.Count(c => c.ClanTag == leagueWar.WarClans[1].ClanTag) == 1)
                    break;

                leagueGroup = null;
            }

            if (leagueGroup == null)
                throw new CocApiException("The LeagueGroup could not be found.");

            return leagueGroup.GroupKey();
        }

        public async Task<ILeagueGroup?> GetLeagueGroupAsync(string clanTag, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
            => await CocApi.GetOrFetchAsync<ILeagueGroup>(LeagueGroup.Url(clanTag), cacheOption, cancellationToken).ConfigureAwait(false);

        public async Task<IWar?> GetWarLogAsync(string clanTag, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
            => await CocApi.GetOrFetchAsync<IWar>(WarLogEntry.Url(clanTag), cacheOption, cancellationToken).ConfigureAwait(false);

        public async Task<CurrentWar?> GetActiveWarAsync(string clanTag)
        {
            if (CocApiClient_old.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            CachedWar? cachedWar = await CocApi.SqlWriter.Select<CachedWar>()
                                                        .Where(c => (c.ClanTag == clanTag ||
                                                                    c.OpponentTag == clanTag) &&
                                                                    c.EndTime > DateTime.UtcNow)
                                                        .OrderBy(c => c.PrepStartTime)
                                                        .QueryFirstOrDefaultAsync()
                                                        .ConfigureAwait(false);

            if (cachedWar != null)
                return cachedWar?.Json.Deserialize<IWar>() as CurrentWar;

            cachedWar = await CocApi.SqlWriter.Select<CachedWar>()
                                                        .Where(c => (c.ClanTag == clanTag ||
                                                                    c.OpponentTag == clanTag))
                                                        .OrderByDesc(c => c.PrepStartTime)
                                                        .QueryFirstOrDefaultAsync()
                                                        .ConfigureAwait(false);

            return cachedWar?.Json.Deserialize<IWar>() as CurrentWar;
        }

        public async Task<ImmutableArray<CurrentWar>> GetWarsAsync(string clanTag)
        {
            if (CocApiClient_old.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            var cachedWars = await CocApi.SqlWriter.Select<CachedWar>()
                                                   .Where(c => c.ClanTag == clanTag ||
                                                               c.OpponentTag == clanTag)
                                                   .QueryToListAsync()
                                                   .ConfigureAwait(false);

            List<CurrentWar> results = new List<CurrentWar>();

            foreach (CachedWar cachedWar in cachedWars)
                results.Add((CurrentWar) cachedWar.Json.Deserialize<IWar>()!);

            return results.ToImmutableArray();
        }



        internal void OnAddedToWarLog(WarLogEntry warLogEntry, CurrentWar storedWar) => AddedToWarLog?.Invoke(this, new ChangedEventArgs<WarLogEntry, CurrentWar>(warLogEntry, storedWar));

        internal void OnNewAttacks(CurrentWar fetched, List<Attack> attacks)
        {
            if (attacks.Count > 0)            
                NewAttacks?.Invoke(this, new ChangedEventArgs<CurrentWar, ImmutableArray<Attack>>(fetched, attacks.ToImmutableArray()));            
        }

        internal void OnNewWar(CurrentWar fetched)
            => NewWar?.Invoke(this, new EventArgs<CurrentWar>(fetched));

        internal void OnWarChanged(CurrentWar fetched, CurrentWar queued)
            => WarChanged?.Invoke(this, new ChangedEventArgs<CurrentWar, CurrentWar>(fetched, queued));

        internal void OnWarEnded(CurrentWar fetched)
            => WarEnded?.Invoke(this, new EventArgs<CurrentWar>(fetched));

        internal void OnWarEndingSoon(CurrentWar fetched)
            => WarEndingSoon?.Invoke(this, new EventArgs<CurrentWar>(fetched));

        internal void OnWarEndNotSeen(CurrentWar fetched)
            => WarEndNotSeen?.Invoke(this, new EventArgs<CurrentWar>(fetched));

        internal void OnWarEndSeen(CurrentWar currentWar)
            => WarEndSeen?.Invoke(this, new EventArgs<CurrentWar>(currentWar));

        internal void OnWarIsAccessibleChanged(CurrentWar currentWar, bool isAccessible)
            => WarAccessibilityChanged?.Invoke(this, new ChangedEventArgs<CurrentWar, bool>(currentWar, isAccessible));

        internal void OnWarStarted(CurrentWar fetched)
            => WarStarted?.Invoke(this, new EventArgs<CurrentWar>(fetched));

        internal void OnWarStartingSoon(CurrentWar fetched)
            => WarStartingSoon?.Invoke(this, new EventArgs<CurrentWar>(fetched));
    }
}