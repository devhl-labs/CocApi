﻿using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Wars
    {
        private CocApi CocApi { get; }

        private bool StopRequested { get; set; }

        public Wars(CocApi cocApi)
        {
            CocApi = cocApi;

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
        public event AsyncEventHandler<ChangedEventArgs<WarLogEntry, CurrentWar>>? AddedToWarLog;
        public event AsyncEventHandler? QueuePopulated;

        /// <summary>
        /// Controls whether any clan will download league wars.
        /// Set it to Auto to only download on the first week of the month.
        /// </summary>
        public DownloadLeagueWars DownloadLeagueWars { get; set; } = DownloadLeagueWars.Auto;

        public bool QueueRunning { get; private set; }

        internal ConcurrentDictionary<string, Paginated<WarLogEntry>> PaginatedWarLogs { get; } = new ConcurrentDictionary<string, Paginated<WarLogEntry>>();

        internal ConcurrentDictionary<string, WarLogEntry> WarLogs { get; } = new ConcurrentDictionary<string, WarLogEntry>();

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
                if (!(await CocApi.FetchAsync<LeagueWar>(LeagueWar.Url(formattedTag), cancellationToken) is IWar war))
                    return null;

                if (war is LeagueWar leagueWar)
                {
                    leagueWar.WarTag = formattedTag;

                    leagueWar.WarType = WarType.SCCWL;
                }

                return war;
            }

            return await CocApi.FetchAsync<CurrentWar>(CurrentWar.Url(formattedTag), cancellationToken) as IWar;
        }

        public async Task<ILeagueGroup?> FetchLeagueGroupAsync(string clanTag, CancellationToken? cancellationToken = null)
        {
            if (!CocApi.TryGetValidTag(clanTag, out string formattedTag))
                throw new InvalidTagException(clanTag);

            return await CocApi.FetchAsync<LeagueGroup>(LeagueGroup.Url(formattedTag), cancellationToken) as ILeagueGroup;
        }

        public async Task<Paginated<WarLeague>?> FetchWarLeaguesAsync(CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<WarLeague>>(WarLeague.Url(), cancellationToken).ConfigureAwait(false) as Paginated<WarLeague>;

        public Paginated<WarLogEntry>? GetWarLog(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            PaginatedWarLogs.TryGetValue(formattedTag, out Paginated<WarLogEntry>? cached);

            return cached;
        }

        public async Task<Paginated<WarLogEntry>?> GetWarLogAsync(string clanTag, bool allowExpired = true, CancellationToken? cancellationToken = null)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            Paginated<WarLogEntry>? warLog = GetWarLog(formattedTag);

            if (warLog != null && (allowExpired || warLog.IsExpired() == false))
                return warLog;

            warLog = await FetchWarLogAsync(formattedTag, null, null, null, cancellationToken);

            if (warLog != null)
            {
                PaginatedWarLogs.AddOrUpdate(formattedTag, warLog, (_, old) =>
                {
                   return (warLog.ServerExpirationUtc > old.ServerExpirationUtc) ? warLog : old;
                });

                foreach(WarLogEntry warLogEntry in warLog.Items.EmptyIfNull())
                {
                    if (warLogEntry.Clan == null || warLogEntry.Clan.ClanTag == null)
                        continue;

                    WarLogs.TryAdd($"{warLogEntry.EndTimeUtc};{warLogEntry.Clan.ClanTag}", warLogEntry);
                }
            }

            return warLog;
        }

        public List<WarLogEntry>? GetWarLogEntries(string clanTag, string opponentTag, DateTime endTimeUtc)
        {
            List<WarLogEntry> warLogEntries = new List<WarLogEntry>();

            if (WarLogs.TryGetValue($"{endTimeUtc};{clanTag}", out WarLogEntry clan))
                warLogEntries.Add(clan);

            if (WarLogs.TryGetValue($"{endTimeUtc};{opponentTag}", out WarLogEntry opponent))
                warLogEntries.Add(opponent);

            if (warLogEntries.Count == 0)
                return null;

            return warLogEntries;
        }

        public async Task<Paginated<WarLogEntry>?> FetchWarLogAsync(string clanTag, int? limit = null, int? after = null, int? before = null, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<WarLogEntry>>(WarLogEntry.Url(clanTag, limit, after, before), cancellationToken).ConfigureAwait(false) as Paginated<WarLogEntry>;

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

            if (CocApi.Wars.IsDownloadingLeagueWars() == false)
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

            if (cached is CurrentWar currentWar)
            {
                if (currentWar.IsExpired() == false)
                {
                    return cached;
                }
                else if (DateTime.UtcNow > currentWar.EndTimeUtc &&
                    DateTime.UtcNow < currentWar.EndTimeUtc.AddMinutes(10) &&
                    DateTime.UtcNow < currentWar.ServerExpirationUtc)
                {
                    return cached;
                }
                else if (DateTime.UtcNow < currentWar.StartTimeUtc.AddMinutes(-10))
                {
                    return cached;
                }
            }
            else if (cached != null && (allowExpiredItem || cached.IsExpired() == false))
            {
                return cached;
            }

            IWar? fetched = await FetchAsync<T>(formattedTag, cancellationToken).ConfigureAwait(false);

            UpdateWarDictionary(fetched, formattedTag);

            return fetched ?? cached;
        }

        public async Task<IWar?> GetCurrentWarAsync(CurrentWar storedWar, CancellationToken? cancellationToken = null)
        {
            IWar? war;

            if (storedWar is LeagueWar leagueWar)
            {
                war = await GetAsync<LeagueWar>(leagueWar.WarTag, false, cancellationToken).ConfigureAwait(false);

                UpdateWarDictionary(war, leagueWar.WarTag);

                return war;
            }
            else
            {
                List<IWar> wars = new List<IWar>();

                foreach (WarClan warClan in storedWar.WarClans)
                {
                    war = await GetAsync<CurrentWar>(warClan.ClanTag, false, cancellationToken).ConfigureAwait(false);

                    if (war is CurrentWar currentWar && currentWar.WarKey() == storedWar.WarKey()) 
                    {
                        if (currentWar.State == WarState.WarEnded)
                            return currentWar;

                        if (DateTime.UtcNow < currentWar.EndTimeUtc)
                            return currentWar;

                        wars.Add(war);
                    }

                    if (war != null)
                        wars.Add(war);
                }

                if (wars.Where(w => w is CurrentWar cw && cw.WarKey() == storedWar.WarKey())
                                              .OrderByDescending(w => w.ServerExpirationUtc)
                                              .FirstOrDefault() is CurrentWar currentWar1)
                    return currentWar1;

                List<WarLogEntry>? warLogs = GetWarLogEntries(storedWar.WarClans[0].ClanTag, storedWar.WarClans[1].ClanTag, storedWar.EndTimeUtc);

                if (warLogs != null)
                {
                    WarLogEntry? warLog = warLogs.FirstOrDefault(wl => wl.Clan != null &&
                                                                       wl.Clan.ClanTag != null &&
                                                                       wl.Opponent != null &&
                                                                       wl.Opponent.ClanTag != null);
                    if (warLog != null) return warLog;

                    return warLogs.First(wl => wl.Clan != null && wl.Clan.ClanTag != null);
                }

                if (wars.Any(w => w is CurrentWar currentWar1 &&
                    currentWar1.WarKey() != storedWar.WarKey() &&
                    currentWar1.PreparationStartTimeUtc > storedWar.PreparationStartTimeUtc))
                    return new NotInWar();

                if (wars.Any(w => w is PrivateWarLog))
                    return wars.First(w => w is PrivateWarLog);

                if (wars.Any(w => w is NotInWar))
                    return wars.First(w => w is NotInWar);
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
                    CocApi.UpdateDictionary(LeagueGroups, clan.ClanTag, leagueGroup);
            }
            else if (fetched != null)
            {
                CocApi.UpdateDictionary(LeagueGroups, formattedTag, fetched);
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
            QueuedWars.TryAdd(currentWar.WarKey(), currentWar);
        }

        public void Queue(IEnumerable<CurrentWar> currentWars)
        {
            foreach (CurrentWar currentWar in currentWars)
                Queue(currentWar);
        }

        public void StartQueue()
        {
            StopRequested = false;

            if (QueueRunning)
                return;

            QueueRunning = true;

            Task.Run(async () =>
            {
                try
                {
                    while (StopRequested == false)
                    {
                        foreach (Clan? clan in CocApi.Clans.Queued.Values)
                        {
                            if (StopRequested)
                                break;

                            if (clan == null)
                                continue;

                            await PopulateWars(clan).ConfigureAwait(false);
                        }

                        foreach (CurrentWar storedWar in CocApi.Wars.QueuedWars.Values)
                        {
                            if (StopRequested)
                                break;

                            Update(storedWar);
                        }

                        OnQueuePopulated();

                        await Task.Delay(50);
                    }

                    QueueRunning = false;

                    CocApi.OnLog(new LogEventArgs(nameof(Wars), nameof(StartQueue), LogLevel.Information, LoggingEvent.QueueExited.ToString()));
                }
                catch (Exception e)
                {
                    StopRequested = false;

                    QueueRunning = false;

                    CocApi.OnLog(new ExceptionEventArgs(nameof(Wars), nameof(StartQueue), e));

                    _ = CocApi.WarQueueRestartAsync();

                    throw e;
                }
            });
        }

        public void StopQueue() => StopRequested = true;

        public Task StopQueueAsync()
        {
            StopRequested = true;

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

        internal void OnQueuePopulated()
        {
            if (CocApi.Clans.ClanUpdateGroups.All(g => g.QueueIsPopulated))
                QueuePopulated?.Invoke(this, EventArgs.Empty);
        }

        internal void OnAddedToWarLog(WarLogEntry warLogEntry, CurrentWar storedWar) => AddedToWarLog?.Invoke(this, new ChangedEventArgs<WarLogEntry, CurrentWar>(warLogEntry, storedWar));

        private async Task PopulateWars(Clan clan)
        {
            if (clan.QueueCurrentWar)
            {
                IWar? war = null;

                if (clan.IsWarLogPublic)
                {
                    war = await CocApi.Wars.GetAsync<CurrentWar>(clan.ClanTag, allowExpiredItem: false).ConfigureAwait(false);
                }
                else
                {
                    war = GetActiveWar(clan.ClanTag);

                    if (war is CurrentWar currentWar1)
                        war = await CocApi.Wars.GetAsync<CurrentWar>(currentWar1.WarClans.First(wc => wc.ClanTag != clan.ClanTag).ClanTag, false).ConfigureAwait(false);
                }

                if (war is CurrentWar currentWar)
                {
                    if (CocApi.Wars.QueuedWars.TryAdd(currentWar.WarKey(), currentWar))
                        OnNewWar(currentWar);

                    if (DateTime.UtcNow > currentWar.EndTimeUtc &&
                        currentWar.State != WarState.WarEnded &&
                        clan.IsWarLogPublic)
                        await CocApi.Wars.GetAsync<CurrentWar>(currentWar.WarClans.First(wc => wc.ClanTag != clan.ClanTag).ClanTag, allowExpiredItem: false).ConfigureAwait(false);
                }

                if (!(war is PrivateWarLog))
                    await GetWarLogAsync(clan.ClanTag);
            }

            if (clan.QueueLeagueWars && CocApi.Wars.IsDownloadingLeagueWars())
            {
                List<LeagueWar>? leagueWars = await CocApi.Wars.GetLeagueWarsAsync(clan.ClanTag, false).ConfigureAwait(false);

                foreach (LeagueWar leagueWar in leagueWars.EmptyIfNull())
                    if (CocApi.Wars.QueuedWars.TryAdd(leagueWar.WarKey(), leagueWar))
                        OnNewWar(leagueWar);
            }
        }

        private void Update(CurrentWar queued)
        {
            if (CachedWars.TryGetValue(queued.WarKey(), out IWar? fetched))
                queued.Update(CocApi, fetched);

            if (WarLogs.TryGetValue($"{queued.EndTimeUtc};{queued.WarClans[0].ClanLevel}", out WarLogEntry entry))
            {                 
                queued.Update(CocApi, entry);
            }
            else
            {
                if (WarLogs.TryGetValue($"{queued.EndTimeUtc};{queued.WarClans[1].ClanLevel}", out entry))
                {
                    queued.Update(CocApi, entry);
                }
            }

            if (fetched is CurrentWar fetchedWar)
            {
                CocApi.UpdateDictionary(QueuedWars, fetchedWar.WarKey(), fetchedWar);
            }
        }

        private void UpdateWarDictionary(IWar? war, string formattedTag)
        {
            if (war is LeagueWar leagueWar)
            {
                CocApi.UpdateDictionary(CachedWars, leagueWar.WarTag, leagueWar);
            }

            if (war is CurrentWar currentWar)
            {
                if (currentWar.State == WarState.Preparation || currentWar.State == WarState.InWar || currentWar.WarKey() == GetActiveWar(currentWar.WarClans.First().ClanTag)?.WarKey())
                {
                    foreach (WarClan warClan in currentWar.WarClans)
                        CocApi.UpdateDictionary(CachedWars, warClan.ClanTag, currentWar);
                }

                CocApi.UpdateDictionary(CachedWars, currentWar.WarKey(), currentWar);
            }

            CocApi.UpdateDictionary(CachedWars, formattedTag, war);
        }
    }
}