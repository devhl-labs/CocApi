using CocApi.Cache.Models;
using CocApi.Cache.Models.Villages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Cache
{
    public class Villages_old
    {
        private CocApiClient_old CocApi { get; }

        public Villages_old(CocApiClient_old cocApi)
        {
            CocApi = cocApi;
        }

        public event AsyncEventHandler<ChangedEventArgs<Village, ImmutableArray<Achievement>>>? AchievementsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, ImmutableArray<Troop>>>? HeroesChanged;
        public event AsyncEventHandler<LabelsChangedEventArgs<Village>>? LabelsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village>>? LegendLeagueChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, ImmutableArray<Spell>>>? SpellsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village, ImmutableArray<Troop>>>? TroopsChanged;
        public event AsyncEventHandler<ChangedEventArgs<Village>>? VillageChanged;


        public async Task<Village?> GetAsync(string villageTag, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
            => await CocApi.GetOrFetchAsync<Village>(Village.Url(villageTag), cacheOption, cancellationToken).ConfigureAwait(false);

        public async Task<Paginated<TopBuilderVillage>?> GetTopBuilderVillagesAsync(int? locationId = null, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
            => await CocApi.GetOrFetchAsync<Paginated<TopBuilderVillage>>(TopBuilderVillage.Url(locationId), cacheOption, cancellationToken).ConfigureAwait(false);

        public async Task<Paginated<TopMainVillage>?> GetTopVillagesAsync(int? locationId = null, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
            => await CocApi.GetOrFetchAsync<Paginated<TopMainVillage>>(TopMainVillage.Url(locationId), cacheOption, cancellationToken).ConfigureAwait(false);



        internal void OnVillageAchievementsChanged(Village fetched, List<Achievement> achievements) 
            => AchievementsChanged?.Invoke(this, new ChangedEventArgs<Village, ImmutableArray<Achievement>>(fetched, achievements.ToImmutableArray()));

        internal void OnVillageChanged(Village fetched, Village queued)
            => VillageChanged?.Invoke(this, new ChangedEventArgs<Village>(fetched, queued));

        internal void OnVillageHeroesChanged(Village fetched, List<Troop> heroes) 
            => HeroesChanged?.Invoke(this, new ChangedEventArgs<Village, ImmutableArray<Troop>>(fetched, heroes.ToImmutableArray()));

        internal void OnVillageLabelsChanged(Village fetched, List<Label> addedLabels, List<Label> removedLabels)
        {
            if (addedLabels.Count == 0 && removedLabels.Count == 0)
                return;

            LabelsChanged?.Invoke(this, new LabelsChangedEventArgs<Village>(fetched, addedLabels.ToImmutableArray(), removedLabels.ToImmutableArray()));
        }

        internal void OnVillageLegendLeagueChanged(Village fetched, Village queued) 
            => LegendLeagueChanged?.Invoke(this, new ChangedEventArgs<Village>(fetched, queued));

        internal void OnVillageSpellsChanged(Village fetched, List<Spell> spells) 
            => SpellsChanged?.Invoke(this, new ChangedEventArgs<Village, ImmutableArray<Spell>>(fetched, spells.ToImmutableArray()));

        internal void OnVillageTroopsChanged(Village fetched, List<Troop> troops) 
            => TroopsChanged?.Invoke(this, new ChangedEventArgs<Village, ImmutableArray<Troop>>(fetched, troops.ToImmutableArray()));

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
                fetched.League?.Id != stored.League?.Id ||
                fetched.Role != stored.Role ||
                fetched.ClanTag != stored.ClanTag)
            {
                return true;
            }

            return false;
        });
    }
}