using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Wars
    {
        private readonly CocApi _cocApi;

        private bool _stopRequested = false;

        public Wars(CocApi cocApi)
        {
            _cocApi = cocApi;

            //_warUpdateService = new WarUpdateService(_cocApi);
        }

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
        public event AsyncEventHandler<ChangedEventArgs<CurrentWar, WarLogEntry>>? FinalAttacksNotSeen;
        public event AsyncEventHandler? WarQueueCompleted;

        /// <summary>
        /// Controls whether any clan will download league wars.
        /// Set it to Auto to only download on the first week of the month.
        /// </summary>
        public DownloadLeagueWars DownloadLeagueWars { get; set; } = DownloadLeagueWars.Auto;

        public bool QueueRunning { get; private set; } = false;

        /// <summary>
        /// One war will appear in this dictionary multiple times using both clantags, warkey, and wartag
        /// </summary>
        internal ConcurrentDictionary<string, IWar?> CachedWars { get; } = new ConcurrentDictionary<string, IWar?>();

        internal ConcurrentDictionary<string, ILeagueGroup?> LeagueGroups { get; } = new ConcurrentDictionary<string, ILeagueGroup?>();

        /// <summary>
        /// Wars will appear one time with the WarKey as the key
        /// </summary>
        internal ConcurrentDictionary<string, CurrentWar> QueuedWars { get; } = new ConcurrentDictionary<string, CurrentWar>();

        public async Task<IWar?> FetchAsync<T>(string tag, CancellationToken? cancellationToken = null) where T : CurrentWar
        {
            if (!CocApi.TryGetValidTag(tag, out string formattedTag))
                throw new InvalidTagException(tag);

            if (typeof(T) == typeof(LeagueWar))
            {
                if (!(await _cocApi.FetchAsync<LeagueWar>(LeagueWar.Url(formattedTag), cancellationToken) is IWar war))
                    return null;

                if (war is LeagueWar leagueWar)
                {
                    leagueWar.WarTag = formattedTag;

                    leagueWar.WarType = WarType.SCCWL;
                }

                return war;
            }

            return await _cocApi.FetchAsync<CurrentWar>(CurrentWar.Url(formattedTag), cancellationToken) as IWar;
        }

        public async Task<ILeagueGroup?> FetchLeagueGroupAsync(string clanTag, CancellationToken? cancellationToken = null)
        {
            if (!CocApi.TryGetValidTag(clanTag, out string formattedTag))
                throw new InvalidTagException(clanTag);

            return await _cocApi.FetchAsync<LeagueGroup>(LeagueGroup.Url(formattedTag), cancellationToken) as ILeagueGroup;
        }

        public async Task<Paginated<WarLeague>?> FetchWarLeaguesAsync(CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<WarLeague>>(WarLeague.Url(), cancellationToken).ConfigureAwait(false) as Paginated<WarLeague>;

        public async Task<Paginated<WarLogEntry>?> FetchWarLogAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<WarLogEntry>>(WarLogEntry.Url(clanTag, limit, after, before), cancellationToken).ConfigureAwait(false) as Paginated<WarLogEntry>;

        public IWar? Get(string tag)
        {
            CocApi.TryGetValidTag(tag, out string formattedTag);

            CachedWars.TryGetValue(formattedTag ?? tag, out IWar? cached);

            return cached;
        }

        public CurrentWar? GetActiveWar(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            IWar? war = Get(formattedTag);

            if (_cocApi.Wars.IsDownloadingLeagueWars() == false)
                return war as CurrentWar;

            if (war is CurrentWar currentWar && (currentWar.State == WarState.Preparation || currentWar.State == WarState.InWar))
                return currentWar;

            List<LeagueWar>? leagueWars = GetLeagueWars(formattedTag);

            if (leagueWars == null)
                return war as CurrentWar;

            LeagueWar? leagueWarInPrep = null;

            foreach (LeagueWar leagueWar in leagueWars)
            {
                if (leagueWar.State == WarState.InWar)
                    return leagueWar;

                if (leagueWar.State == WarState.Preparation)
                    leagueWarInPrep = leagueWar;
            }

            if (leagueWarInPrep != null)
                return leagueWarInPrep;

            if (war != null && war is CurrentWar currentWar1 && leagueWars.Last().PreparationStartTimeUtc > currentWar1.PreparationStartTimeUtc)
                return leagueWars.Last();

            return war as CurrentWar;
        }

        public async Task<IWar?> GetAsync<T>(string tag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null) where T : CurrentWar
        {
            if (CocApi.TryGetValidTag(tag, out string formattedTag) == false)
                throw new InvalidTagException(tag);

            CachedWars.TryGetValue(formattedTag, out IWar? cached);

            if (cached != null && (allowExpiredItem || cached.IsExpired() == false))
                return cached;

            IWar? fetched = await FetchAsync<T>(formattedTag, cancellationToken).ConfigureAwait(false);

            UpdateWarDictionary(fetched, formattedTag);

            return fetched ?? cached;
        }

        public async Task<IWar?> GetCurrentWarAsync(CurrentWar storedWar, CancellationToken? cancellationToken = null)
        {
            IWar? war;

            if (storedWar is LeagueWar leagueWar)
            {
                war = await GetAsync<LeagueWar>(leagueWar.WarTag, false, cancellationToken);

                UpdateWarDictionary(war, leagueWar.WarTag);

                return war;
            }
            else
            {
                List<IWar> wars = new List<IWar>();

                foreach (WarClan warClan in storedWar.WarClans)
                {
                    war = await GetAsync<CurrentWar>(warClan.ClanTag, false, cancellationToken);

                    if (war is CurrentWar currentWar && currentWar.WarKey == storedWar.WarKey)
                        return war;

                    if (war != null)
                        wars.Add(war);
                }

                if (wars.Any(w => w is CurrentWar currentWar1 &&
                    currentWar1.WarKey != storedWar.WarKey &&
                    currentWar1.PreparationStartTimeUtc > storedWar.PreparationStartTimeUtc))
                    return new NotInWar();

                if (wars.Any(w => w is NotInWar))
                    return wars.First(w => w is NotInWar);

                if (wars.Any(w => w is PrivateWarLog))
                    return wars.First(w => w is PrivateWarLog);
            }

            return null;
        }

        public ILeagueGroup? GetLeagueGroup(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            LeagueGroups.TryGetValue(formattedTag, out ILeagueGroup? result);

            return result;
        }

        public async Task<ILeagueGroup?> GetLeagueGroupAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            ILeagueGroup? cached = GetLeagueGroup(formattedTag);

            if (cached != null && (allowExpiredItem || cached.IsExpired() == false))
                return cached;

            ILeagueGroup? fetched = await FetchLeagueGroupAsync(formattedTag, cancellationToken).ConfigureAwait(false);

            if (fetched is LeagueGroup leagueGroup)
            {
                foreach (var clan in leagueGroup.Clans.EmptyIfNull())
                    _cocApi.UpdateDictionary(LeagueGroups, clan.ClanTag, leagueGroup);
            }
            else if (fetched != null)
            {
                _cocApi.UpdateDictionary(LeagueGroups, formattedTag, fetched);
            }

            return fetched ?? cached;
        }

        public List<LeagueWar>? GetLeagueWars(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            List<LeagueWar>? results = new List<LeagueWar>();

            ILeagueGroup? iLeagueGroup = GetLeagueGroup(formattedTag);

            if (!(iLeagueGroup is LeagueGroup leagueGroup))
                return null;

            foreach (Round round in leagueGroup.Rounds.EmptyIfNull())
            {
                foreach (string warTag in round.WarTags.EmptyIfNull())
                {
                    if (CachedWars.TryGetValue(warTag, out IWar? war) && war is LeagueWar leagueWar && leagueWar.WarClans.Any(c => c.ClanTag == formattedTag))
                    {
                        results.Add(leagueWar);
                    }
                }
            }

            return results;
        }

        public async Task<List<LeagueWar>?> GetLeagueWarsAsync(string clanTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            if (!(await GetLeagueGroupAsync(formattedTag, allowExpiredItem, cancellationToken).ConfigureAwait(false) is LeagueGroup leagueGroup))
                return null;

            List<LeagueWar>? leagueWars = await GetLeagueWarsAsync(leagueGroup, allowExpiredItem, cancellationToken).ConfigureAwait(false);

            if (leagueWars.Count() == 0)
                return null;

            leagueWars = leagueWars.Where(w => w.WarClans.Any(wc => wc.ClanTag == formattedTag)).ToList();

            if (leagueWars.Count() == 0)
                return null;

            return leagueWars;
        }

        public async Task<List<LeagueWar>?> GetLeagueWarsAsync(LeagueGroup leagueGroup, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            List<LeagueWar> leagueWars = new List<LeagueWar>();

            List<Task> tasks = new List<Task>();

            foreach (var round in leagueGroup.Rounds.EmptyIfNull())
            {
                foreach (string warTag in round.WarTags.EmptyIfNull().Where(tag => tag != "#0"))
                {
                    tasks.Add(GetAsync<LeagueWar>(warTag, allowExpiredItem, cancellationToken));
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach(Task<IWar?> task in tasks)
            {
                if (await task is LeagueWar war)
                    leagueWars.Add(war);
            }

            if (leagueWars.Count() == 0)
                return null;

            return leagueWars.OrderBy(w => w.PreparationStartTimeUtc).ToList();
        }

        /// <summary>
        /// Determines whether CWL should be downloading.
        /// When DownloadLeagueWars is set to Auto, this returns true during the first week of the month
        /// and the first three hours of day 8.  This is just to give you time to complete the downloads.
        /// </summary>
        /// <returns></returns>
        public bool IsDownloadingLeagueWars()
        {
            if (DownloadLeagueWars == DownloadLeagueWars.False)
                return false;

            if (DownloadLeagueWars == DownloadLeagueWars.True)
                return true;

            if (DownloadLeagueWars == DownloadLeagueWars.Auto)
            {
                int day = DateTime.UtcNow.Day;

                if (day > 0 && day < 11)
                {
                    return true;
                }

                //just to ensure we get everything we need
                if (day == 11 && DateTime.UtcNow.Hour < 3)
                {
                    return true;
                }
            }

            return false;
        }

        public void Queue(CurrentWar currentWar)
        {
            QueuedWars.TryAdd(currentWar.WarKey, currentWar);
        }

        public void Queue(IEnumerable<CurrentWar> currentWars)
        {
            foreach (CurrentWar currentWar in currentWars)
                Queue(currentWar);
        }

        public void StartQueue()
        {
            _stopRequested = false;

            if (QueueRunning)
                return;

            QueueRunning = true;

            Task.Run(async () =>
            {
                try
                {
                    while (_stopRequested == false)
                    {
                        foreach (Clan? clan in _cocApi.Clans.Queued.Values)
                        {
                            if (clan == null)
                                continue;

                            await PopulateWars(clan).ConfigureAwait(false);

                            if (_stopRequested)
                                break;
                        }

                        foreach (CurrentWar storedWar in _cocApi.Wars.QueuedWars.Values)
                        {
                            await Update(storedWar).ConfigureAwait(false);

                            if (_stopRequested)
                                break;
                        }

                        OnWarQueueCompleted();

                        await Task.Delay(50);
                    }

                    QueueRunning = false;

                    _cocApi.OnLog(new LogEventArgs(nameof(Wars), nameof(StartQueue), LogLevel.Information, LoggingEvent.QueueExited.ToString()));
                }
                catch (Exception e)
                {
                    _stopRequested = false;

                    QueueRunning = false;

                    _cocApi.OnLog(new ExceptionLogEventArgs(nameof(Wars), nameof(StartQueue), e));

                    _ = _cocApi.WarQueueRestartAsync();

                    throw e;
                }
            });
        }

        public void StopQueue() => _stopRequested = false;

        public Task StopQueueAsync()
        {
            _stopRequested = false;

            TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();

            Task task = tsc.Task;

            _ = Task.Run(async () =>
            {
                while (QueueRunning)
                    await Task.Delay(100).ConfigureAwait(false);

                tsc.SetResult(true);
            });

            return tsc.Task;
        }

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

        internal void OnWarQueueCompleted() => WarQueueCompleted?.Invoke(this, EventArgs.Empty);

        internal void OnFinalAttacksNotSeen(CurrentWar storedWar, WarLogEntry warLogEntry) => FinalAttacksNotSeen?.Invoke(this, new ChangedEventArgs<CurrentWar, WarLogEntry>(storedWar, warLogEntry));

        private async Task PopulateWars(Clan clan)
        {
            if (clan.QueueCurrentWar && clan.IsWarLogPublic)
            {
                IWar? war = await _cocApi.Wars.GetAsync<CurrentWar>(clan.ClanTag, allowExpiredItem: false).ConfigureAwait(false);

                if (war is CurrentWar currentWar)
                {
                    if (_cocApi.Wars.QueuedWars.TryAdd(currentWar.WarKey, currentWar))
                        OnNewWar(currentWar);
                }
            }

            if (clan.QueueLeagueWars && _cocApi.Wars.IsDownloadingLeagueWars())
            {
                List<LeagueWar>? leagueWars = await _cocApi.Wars.GetLeagueWarsAsync(clan.ClanTag, false).ConfigureAwait(false);

                foreach (LeagueWar leagueWar in leagueWars.EmptyIfNull())
                {
                    if (_cocApi.Wars.QueuedWars.TryAdd(leagueWar.WarKey, leagueWar))
                        OnNewWar(leagueWar);
                }
            }
        }

        private async Task Update(CurrentWar queued)
        {
            if (queued.IsExpired() == false)
                return;

            WarLogEntry? warLogEntry = null;

            if (await _cocApi.Wars.GetCurrentWarAsync(queued).ConfigureAwait(false) is CurrentWar fetched)
            {
                if ((queued.Announcements.HasFlag(Announcements.WarEndSeen) == false && 
                    queued.Announcements.HasFlag(Announcements.WarLogSearched) == false) &&
                    queued.State == WarState.InWar &&
                    DateTime.UtcNow > queued.EndTimeUtc &&
                    (fetched == null || (fetched is CurrentWar fetchedWar && fetchedWar.WarKey != queued.WarKey)))
                {
                    foreach(WarClan warClan in queued.WarClans)
                    {
                        Paginated<WarLogEntry>? logs = await _cocApi.Wars.FetchWarLogAsync(warClan.ClanTag);

                        warLogEntry = logs?.Items.FirstOrDefault(e => e.EndTimeUtc == queued.EndTimeUtc && e.WarClans.First().Result != Result.Null);

                        if (warLogEntry != null)
                            break;
                    }
                }

                if (warLogEntry != null)
                {
                    queued.Update(_cocApi, warLogEntry);
                }
                else
                {
                    queued.Update(_cocApi, fetched);
                }

                if (fetched != null)
                    _cocApi.UpdateDictionary(QueuedWars, fetched.WarKey, fetched);
            }
        }

        private void UpdateWarDictionary(IWar? war, string formattedTag)
        {
            if (war is LeagueWar leagueWar)
            {
                _cocApi.UpdateDictionary(CachedWars, leagueWar.WarTag, leagueWar);
            }

            if (war is CurrentWar currentWar)
            {
                if (currentWar.State == WarState.Preparation || currentWar.State == WarState.InWar || currentWar.WarKey == GetActiveWar(currentWar.WarClans.First().ClanTag)?.WarKey)
                {
                    foreach (WarClan warClan in currentWar.WarClans)
                        _cocApi.UpdateDictionary(CachedWars, warClan.ClanTag, currentWar);
                }

                _cocApi.UpdateDictionary(CachedWars, currentWar.WarKey, currentWar);
            }

            _cocApi.UpdateDictionary(CachedWars, formattedTag, war);
        }
    }
}