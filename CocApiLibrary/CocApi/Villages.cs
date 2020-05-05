using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Village;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Villages
    {
        public event AsyncEventHandler<ChangedEventArgs<Village, IReadOnlyList<Achievement>>>? AchievementsChanged;
        /// <summary>
        /// Fires if the following properties change:
        /// <list type="bullet">
        ///    <item><description><see cref="Village.AttackWins"/></description></item>
        ///    <item><description><see cref="Village.BestTrophies"/></description></item>
        ///    <item><description><see cref="Village.BestVersusTrophies"/></description></item>
        ///    <item><description><see cref="Village.BuilderHallLevel"/></description></item>
        ///    <item><description><see cref="Village.TownHallLevel"/></description></item>
        ///    <item><description><see cref="Village.TownHallWeaponLevel"/></description></item>
        ///    <item><description><see cref="Village.WarStars"/></description></item>
        /// </list>
        /// </summary>
        public event AsyncEventHandler<ChangedEventArgs<Village, Village>>? VillageChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, int>>? DefenseWinsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, int>>? ExpLevelChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, IReadOnlyList<Troop>>>? HeroesChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village>>? ReachedLegendsLeague;
        public event AsyncEventHandler<ChangedEventArgs<Village, IReadOnlyList<Spell>>>? SpellsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, IReadOnlyList<Troop>>>? TroopsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, int>>? TrophiesChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, int>>? VersusBattleWinCountChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, int>>? VersusBattleWinsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, int>>? VersusTrophiesChanged;
        public event AsyncEventHandler<LabelsChangedEventArgs<Village, VillageLabel>>? LabelsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, Village>>? ClanChanged;
        public event AsyncEventHandler? VillageQueueCompleted;

        private readonly CocApi _cocApi;

        public Villages(CocApi cocApi)
        {
            _cocApi = cocApi;
        }

        private ConcurrentDictionary<string, Village?> Queued { get; } = new ConcurrentDictionary<string, Village?>();

        internal ConcurrentDictionary<string, Village> Fetched { get; } = new ConcurrentDictionary<string, Village>();

        public async Task<Village?> FetchAsync(string villageTag, CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Village>(Village.Url(villageTag), cancellationToken) as Village;

        public async Task<Paginated<TopBuilderVillage>?> FetchTopBuilderVillagesAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<TopBuilderVillage>>(TopBuilderVillage.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopBuilderVillage>;

        public async Task<Paginated<TopMainVillage>?> FetchTopMainVillagesAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<TopMainVillage>>(TopMainVillage.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopMainVillage>;

        public Village? Get(string villageTag)
        {
            if (CocApi.TryGetValidTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            Queued.TryGetValue(formattedTag, out Village? queued);

            return queued;
        }

        public async Task<Village?> GetAsync(string villageTag, bool allowExpiredItem = true, CancellationToken? cancellationToken = null)
        {
            if (CocApi.TryGetValidTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            Village? queued = Get(formattedTag);

            if (queued != null && (allowExpiredItem || queued.IsExpired() == false))
                return queued;

            Village? fetched = await FetchAsync(formattedTag, cancellationToken);

            if (fetched != null)
                _cocApi.UpdateDictionary(Fetched, fetched.VillageTag, fetched);

            return fetched ?? queued;
        }

        internal void OnClanChanged(Village fetched, Village queued) => ClanChanged?.Invoke(this, new ChangedEventArgs<Village, Village>(fetched, queued));

        internal void OnVillageAchievementsChanged(Village village, List<Achievement> achievements) => AchievementsChanged?.Invoke(this, new ChangedEventArgs<Village, IReadOnlyList<Achievement>>(village, achievements.ToImmutableArray()));

        internal void VillageChangedEvent(Village oldVillage, Village newVillage) => VillageChanged?.Invoke(this, new ChangedEventArgs<Village, Village>(oldVillage, newVillage));

        internal void VillageDefenseWinsChangedEvent(Village village, int increase) => DefenseWinsChanged?.Invoke(this, new ChangedEventArgs<Village, int>(village, increase));

        internal void VillageExpLevelChangedEvent(Village village, int increase) => ExpLevelChanged?.Invoke(this, new ChangedEventArgs<Village, int>(village, increase));

        internal void VillageHeroesChangedEvent(Village village, List<Troop> heroes) => HeroesChanged?.Invoke(this, new ChangedEventArgs<Village, IReadOnlyList<Troop>>(village, heroes.ToImmutableArray()));

        internal void VillageLabelsChangedEvent(Village village, List<VillageLabel> addedLabels, List<VillageLabel> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0)
                return;

            LabelsChanged?.Invoke(this, new LabelsChangedEventArgs<Village, VillageLabel>(village, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray()));
        }

        internal void VillageReachedLegendsLeagueEvent(Village village) => ReachedLegendsLeague?.Invoke(this, new ChangedEventArgs<Village>(village));
         
        internal void VillageSpellsChangedEvent(Village village, List<Spell> spells) => SpellsChanged?.Invoke(this, new ChangedEventArgs<Village, IReadOnlyList<Spell>>(village, spells.ToImmutableArray()));

        internal void VillageTroopsChangedEvent(Village village, List<Troop> troops) => TroopsChanged?.Invoke(this, new ChangedEventArgs<Village, IReadOnlyList<Troop>>(village, troops.ToImmutableArray()));

        internal void VillageTrophiesChangedEvent(Village village, int increase) => TrophiesChanged?.Invoke(this, new ChangedEventArgs<Village, int>(village, increase));

        internal void VillageVersusBattleWinCountChangedEvent(Village village, int increase) => VersusBattleWinCountChanged?.Invoke(this, new ChangedEventArgs<Village, int>(village, increase));

        internal void VillageVersusBattleWinsChangedEvent(Village village, int increase) => VersusBattleWinsChanged?.Invoke(this, new ChangedEventArgs<Village, int>(village, increase));

        internal void VillageVersusTrophiesChangedEvent(Village village, int increase) => VersusTrophiesChanged?.Invoke(this, new ChangedEventArgs<Village, int>(village, increase));



        private bool _stopRequested = false;

        public bool QueueRunning { get; private set; }

        public void StopQueue() => _stopRequested = true;

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
                        foreach(var entry in Queued)
                        {
                            if (entry.Value == null)
                            {
                                await PopulateVillageAsync(entry.Key).ConfigureAwait(false);
                            }
                            else
                            {
                                await UpdateVillageAsync(entry.Value).ConfigureAwait(false);
                            }
                            
                            await Task.Delay(50);
                        }

                        VillageQueueCompleted?.Invoke(this, EventArgs.Empty);
                    }

                    QueueRunning = false;

                    _cocApi.LogEvent<Villages>(logLevel: LogLevel.Information, loggingEvent: LoggingEvent.VillageUpdateEnded);
                }
                catch (Exception e)
                {
                    _stopRequested = false;

                    QueueRunning = false;

                    _cocApi.LogEvent<Villages>(e, LogLevel.Critical, LoggingEvent.QueueCrashed);

                    _ = _cocApi.VillageQueueRestartAsync();

                    throw e;
                }
            });
        }

        private async Task UpdateVillageAsync(Village queued)
        {
            if (queued.IsExpired() == false)
                return;

            Village? fetched = await FetchAsync(queued.VillageTag).ConfigureAwait(false);

            if (fetched != null)
            {
                fetched.Update(_cocApi, queued);

                _cocApi.UpdateDictionary(Queued, fetched.VillageTag, fetched);
            }
        }

        private async Task PopulateVillageAsync(string villageTag)
        {
            Village? fetched = await FetchAsync(villageTag).ConfigureAwait(false);

            if (fetched != null)
                _cocApi.UpdateDictionary(Queued, fetched.VillageTag, fetched);
        }

        public void Queue(string villageTag, string clanTag)
        {
            Village village = new Village
            {
                VillageTag = villageTag,

                ClanTag = clanTag
            };

            Queued.TryAdd(villageTag, village);
        }


    }
}