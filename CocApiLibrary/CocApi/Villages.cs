using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Cache;
using devhl.CocApi.Models.Village;
using Newtonsoft.Json;
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

        public async Task<Village?> FetchAsync(string villageTag, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Village>(Village.Url(villageTag), cancellationToken).ConfigureAwait(false) as Village;

        public async Task<Paginated<TopBuilderVillage>?> FetchTopBuilderVillagesAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<TopBuilderVillage>>(TopBuilderVillage.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopBuilderVillage>;

        public async Task<Paginated<TopMainVillage>?> FetchTopMainVillagesAsync(int? locationId = null, CancellationToken? cancellationToken = null)
            => await CocApi.FetchAsync<Paginated<TopMainVillage>>(TopMainVillage.Url(locationId), cancellationToken).ConfigureAwait(false) as Paginated<TopMainVillage>;

        public async Task<Village?> GetAsync(string villageTag)
        {
            if (CocApi.TryGetValidTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            string path = Village.Url(formattedTag);

            Cache? cache = await CocApi.SqlWriter.Select<Cache>()
                                                .Where(c => c.Path == path)
                                                .QueryFirstOrDefaultAsync()
                                                .ConfigureAwait(false);

            if (cache == null)
                return null;

            return JsonConvert.DeserializeObject<Village>(cache.Json);

            //Queued.TryGetValue(formattedTag, out Village? queued);

            //return queued;
        }

        public async Task<Village?> GetOrFetchAsync(string villageTag, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
        {
            if (CocApi.TryGetValidTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            Village? queued = await GetAsync(formattedTag).ConfigureAwait(false);

            if (queued == null)
                return await FetchAsync(formattedTag, cancellationToken).ConfigureAwait(false);

            if (cacheOption == CacheOption.AllowAny)
                return queued;

            //if (cacheOption == CacheOption.AllowLocallyExpired || queued.IsLocallyExpired(CocApi.CocApiConfiguration.VillageTimeToLive) == false)
            //    return queued;

            if (cacheOption == CacheOption.AllowExpiredServerResponse && queued.IsLocallyExpired(CocApi.CocApiConfiguration.VillageTimeToLive) == false)
                return queued;

            Village? fetched = await FetchAsync(formattedTag, cancellationToken).ConfigureAwait(false);

            return fetched ?? queued;
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