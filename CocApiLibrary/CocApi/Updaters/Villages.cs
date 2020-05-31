﻿using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Village;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Villages
    {
        private CocApi CocApi { get; }

        private bool StopRequested { get; set; } = false;

        public Villages(CocApi cocApi)
        {
            CocApi = cocApi;
        }

        public event AsyncEventHandler<ChangedEventArgs<Village, IReadOnlyList<Achievement>>>? AchievementsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, IReadOnlyList<Troop>>>? HeroesChanged;
        public event AsyncEventHandler<LabelsChangedEventArgs<Village, VillageLabel>>? LabelsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, Village>>? LegendLeagueChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, IReadOnlyList<Spell>>>? SpellsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, IReadOnlyList<Troop>>>? TroopsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, Village>>? VillageChanged;
        public event AsyncEventHandler? QueuePopulated;

        public bool QueueRunning { get; private set; }
        private ConcurrentDictionary<string, Village?> Queued { get; } = new ConcurrentDictionary<string, Village?>();

        public async Task<Village?> FetchAsync(string villageTag, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Village>(Village.Url(villageTag), cancellationToken) as Village;

        public async Task<Paginated<TopBuilderVillage>?> FetchTopBuilderVillagesAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<TopBuilderVillage>>(TopBuilderVillage.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopBuilderVillage>;

        public async Task<Paginated<TopMainVillage>?> FetchTopMainVillagesAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<TopMainVillage>>(TopMainVillage.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopMainVillage>;

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

            return fetched ?? queued;
        }

        public void Queue(string villageTag) => Queue(villageTag, null);

        public void Queue(Village village) => Queue(village.VillageTag, village);

        public void Queue(IEnumerable<string> villageTags)
        {
            foreach (string villageTag in villageTags)
                Queue(villageTag, null);
        }

        public void Queue(IEnumerable<Village> villages)
        {
            foreach (Village village in villages)
                Queue(village.VillageTag, village);
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
                        foreach (var entry in Queued)
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

                        OnQueuePopulated();
                    }

                    QueueRunning = false;

                    CocApi.OnLog(new LogEventArgs(nameof(Villages), nameof(StartQueue), LogLevel.Information, LoggingEvent.QueueExited.ToString()));
                }
                catch (Exception e)
                {
                    StopRequested = false;

                    QueueRunning = false;

                    CocApi.OnLog(new ExceptionEventArgs(nameof(Villages), nameof(StartQueue), e));

                    _ = CocApi.VillageQueueRestartAsync();

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

        internal void OnVillageAchievementsChanged(Village fetched, List<Achievement> achievements) 
            => AchievementsChanged?.Invoke(this, new ChangedEventArgs<Village, IReadOnlyList<Achievement>>(fetched, achievements.ToImmutableArray()));

        internal void OnVillageChanged(Village fetched, Village queued)
            => VillageChanged?.Invoke(this, new ChangedEventArgs<Village, Village>(fetched, queued));

        internal void OnVillageHeroesChanged(Village fetched, List<Troop> heroes) 
            => HeroesChanged?.Invoke(this, new ChangedEventArgs<Village, IReadOnlyList<Troop>>(fetched, heroes.ToImmutableArray()));

        internal void OnVillageLabelsChanged(Village fetched, List<VillageLabel> addedLabels, List<VillageLabel> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0)
                return;

            LabelsChanged?.Invoke(this, new LabelsChangedEventArgs<Village, VillageLabel>(fetched, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray()));
        }

        internal void OnVillageLegendLeagueChanged(Village fetched, Village queued) 
            => LegendLeagueChanged?.Invoke(this, new ChangedEventArgs<Village, Village>(fetched, queued));

        internal void OnVillageSpellsChanged(Village fetched, List<Spell> spells) 
            => SpellsChanged?.Invoke(this, new ChangedEventArgs<Village, IReadOnlyList<Spell>>(fetched, spells.ToImmutableArray()));

        internal void OnVillageTroopsChanged(Village fetched, List<Troop> troops) 
            => TroopsChanged?.Invoke(this, new ChangedEventArgs<Village, IReadOnlyList<Troop>>(fetched, troops.ToImmutableArray()));

        internal void OnQueuePopulated() => QueuePopulated?.Invoke(this, EventArgs.Empty);
        

        private async Task PopulateVillageAsync(string villageTag)
        {
            Village? fetched = await FetchAsync(villageTag).ConfigureAwait(false);

            if (fetched != null)
                CocApi.UpdateDictionary(Queued, fetched.VillageTag, fetched);
        }

        private void Queue(string villageTag, Village? village) => Queued.TryAdd(villageTag, village);

        private async Task UpdateVillageAsync(Village queued)
        {
            if (queued.IsExpired() == false)
                return;

            Village? fetched = await FetchAsync(queued.VillageTag).ConfigureAwait(false);

            if (fetched != null)
            {
                queued.Update(CocApi, fetched);

                CocApi.UpdateDictionary(Queued, fetched.VillageTag, fetched);
            }
        }

        public Func<Village, Village, bool> IsChanged { get; set; } = new Func<Village, Village, bool>((fetched, stored) =>
        {
            if (fetched.AttackWins != stored.AttackWins ||
                fetched.BestTrophies != stored.BestTrophies ||
                fetched.BestVersusTrophies != stored.BestVersusTrophies ||
                fetched.BuilderHallLevel != stored.BuilderHallLevel ||
                fetched.TownHallLevel != stored.TownHallLevel ||
                fetched.TownHallWeaponLevel != stored.TownHallWeaponLevel ||
                fetched.WarStars != stored.WarStars ||
                fetched.DefenseWins != stored.DefenseWins ||
                fetched.ExpLevel != stored.ExpLevel ||
                fetched.Trophies != stored.Trophies ||
                fetched.VersusBattleWinCount != stored.VersusBattleWinCount ||
                fetched.VersusBattleWins != stored.VersusBattleWins ||
                fetched.VersusTrophies != stored.VersusTrophies ||
                fetched.Name != stored.Name ||
                fetched.LeagueId != stored.LeagueId ||
                fetched.Role != stored.Role ||
                fetched.ClanTag != stored.ClanTag)
            {
                return true;
            }

            return false;
        });
    }
}