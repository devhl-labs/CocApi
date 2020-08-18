//using System;
//using System.Collections.Generic;
//using System.Linq;
//using CocApi.Cache.Exceptions;
//using Newtonsoft.Json;

//namespace CocApi.Cache.Models.Villages
//{
//    public class Village : Downloadable, IVillage, IInitialize
//    {
//        public static string Url(string villageTag)
//        {
//            if (CocApiClient_old.TryGetValidTag(villageTag, out string formattedTag) == false)
//                throw new InvalidTagException(villageTag);

//            return $"players/{Uri.EscapeDataString(formattedTag)}";
//        }

//        [JsonProperty("Tag")]
//        public string VillageTag { get; internal set; } = string.Empty;

//        [JsonProperty]
//        public string Name { get; internal set; } = string.Empty;

//        [JsonProperty]
//#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
//        public string? ClanTag { get; internal set; }

//#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

//        [JsonProperty]
//        public int TownHallLevel { get; internal set; }

//        [JsonProperty]
//        public int TownHallWeaponLevel { get; internal set; }

//        [JsonProperty]
//        public int ExpLevel { get; internal set; }

//        [JsonProperty]
//        public int Trophies { get; internal set; }

//        [JsonProperty]
//        public int BestTrophies { get; internal set; }

//        [JsonProperty]
//        public int WarStars { get; internal set; }

//        [JsonProperty]
//        public int AttackWins { get; internal set; }

//        [JsonProperty]
//        public int DefenseWins { get; internal set; }

//        [JsonProperty]
//        public LegendLeagueStatistics? LegendStatistics { get; internal set; }

//        [JsonProperty]
//        public int BuilderHallLevel { get; internal set; }

//        [JsonProperty]
//        public int VersusTrophies { get; internal set; }

//        [JsonProperty]
//        public int BestVersusTrophies { get; internal set; }

//        [JsonProperty]
//        public int VersusBattleWins { get; internal set; }

//        [JsonProperty]
//        public Role Role { get; internal set; } = Role.Unknown;

//        [JsonProperty]
//        public int Donations { get; internal set; }

//        [JsonProperty]
//        public int DonationsReceived { get; internal set; }

//        [JsonProperty]
//        public VillageClan? Clan { get; private set; }

//        [JsonProperty]
//        public League? League { get; internal set; }

//        [JsonProperty]
//        public IEnumerable<Achievement>? Achievements { get; internal set; }

//        [JsonProperty]
//        public int VersusBattleWinCount { get; internal set; }

//        [JsonProperty("Troop")]
//        internal IEnumerable<Troop>? Soldiers { get; set; }

//        [JsonProperty]
//        internal IEnumerable<Troop>? Heroes { get; set; }

//        [JsonProperty]
//        public IList<Troop>? Troops { get; internal set; }

//        [JsonProperty]
//        public IEnumerable<Label>? Labels { get; internal set; }

//        [JsonProperty]
//        public IEnumerable<Spell>? Spells { get; internal set; }




//        internal void Update(CocApiClient_old cocApi, Village fetched)
//        {
//            if (ReferenceEquals(this, fetched)) return;

//            UpdateVillage(cocApi, fetched);

//            UpdateLabels(cocApi, fetched);

//            UpdateVillageAchievements(cocApi, fetched);

//            UpdateVillageTroops(cocApi, fetched);

//            UpdateVillageHeroes(cocApi, fetched);

//            UpdateVillageSpells(cocApi, fetched);

//            UpdateLegendLeagueStatistics(cocApi, fetched);
//        }

//        private void UpdateLegendLeagueStatistics(CocApiClient_old cocApi, Village fetched)
//        {
//            cocApi.OnLog(new VillageLogEventArgs(nameof(Village), nameof(UpdateLegendLeagueStatistics), fetched));

//            if (LegendStatistics == null && fetched.LegendStatistics == null) return;

//            if (LegendStatistics == null && fetched.LegendStatistics != null ||
//                LegendStatistics?.LegendTrophies != fetched.LegendStatistics?.LegendTrophies)
//            {
//                cocApi.Villages.OnVillageLegendLeagueChanged(fetched, this);

//                return;
//            }

//            if (LegendStatistics?.BestSeason?.Id != fetched.LegendStatistics?.BestSeason?.Id ||
//                LegendStatistics?.BestSeason?.Rank != fetched.LegendStatistics?.BestSeason?.Rank ||
//                LegendStatistics?.BestSeason?.Trophies != fetched.LegendStatistics?.BestSeason?.Trophies ||

//                LegendStatistics?.PreviousVersusSeason?.Id != fetched.LegendStatistics?.PreviousVersusSeason?.Id ||
//                LegendStatistics?.PreviousVersusSeason?.Rank != fetched.LegendStatistics?.PreviousVersusSeason?.Rank ||
//                LegendStatistics?.PreviousVersusSeason?.Trophies != fetched.LegendStatistics?.PreviousVersusSeason?.Trophies ||

//                LegendStatistics?.CurrentSeason?.Id != fetched.LegendStatistics?.CurrentSeason?.Id ||
//                LegendStatistics?.CurrentSeason?.Rank != fetched.LegendStatistics?.CurrentSeason?.Rank ||
//                LegendStatistics?.CurrentSeason?.Trophies != fetched.LegendStatistics?.CurrentSeason?.Trophies ||

//                LegendStatistics?.CurrentVersusSeason?.Id != fetched.LegendStatistics?.CurrentVersusSeason?.Id ||
//                LegendStatistics?.CurrentVersusSeason?.Rank != fetched.LegendStatistics?.CurrentVersusSeason?.Rank ||
//                LegendStatistics?.CurrentVersusSeason?.Trophies != fetched.LegendStatistics?.CurrentVersusSeason?.Trophies ||

//                LegendStatistics?.BestVersusSeason?.Id != fetched.LegendStatistics?.BestVersusSeason?.Id ||
//                LegendStatistics?.BestVersusSeason?.Rank != fetched.LegendStatistics?.BestVersusSeason?.Rank ||
//                LegendStatistics?.BestVersusSeason?.Trophies != fetched.LegendStatistics?.BestVersusSeason?.Trophies ||

//                LegendStatistics?.PreviousSeason?.Id != fetched.LegendStatistics?.PreviousSeason?.Id ||
//                LegendStatistics?.PreviousSeason?.Rank != fetched.LegendStatistics?.PreviousSeason?.Rank ||
//                LegendStatistics?.PreviousSeason?.Trophies != fetched.LegendStatistics?.PreviousSeason?.Trophies)
//            {
//                cocApi.Villages.OnVillageLegendLeagueChanged(fetched, this);
//            }
//        }

//        private void UpdateLabels(CocApiClient_old cocApi, Village fetched)
//        {
//            cocApi.OnLog(new VillageLogEventArgs(nameof(Village), nameof(UpdateLabels), fetched));

//            List<Label> added = new List<Label>();

//            List<Label> removed = new List<Label>();

//            foreach(var newLabel in fetched.Labels.EmptyIfNull())
//            {
//                if (Labels.EmptyIfNull().Any(l => l.Id == newLabel.Id) == false)
//                {
//                    added.Add(newLabel);
//                }
//            }

//            foreach(var oldLabel in Labels.EmptyIfNull())
//            {
//                if (fetched.Labels.EmptyIfNull().Any(l => l.Id == oldLabel.Id) == false)
//                {
//                    removed.Add(oldLabel);
//                }
//            }

//            if (Labels == null && fetched.Labels != null && added.Count == 0)
//            {
//                foreach (var newLabel in fetched.Labels)
//                {
//                    added.Add(newLabel);
//                }
//            }

//            if (fetched.Labels == null && Labels != null && removed.Count == 0)
//            {
//                foreach (var removedLabel in Labels)
//                {
//                    removed.Add(removedLabel);
//                }
//            }

//            cocApi.Villages.OnVillageLabelsChanged(fetched, added, removed);
//        }

//        private void UpdateVillageSpells(CocApiClient_old cocApi, Village fetched)
//        {
//            cocApi.OnLog(new VillageLogEventArgs(nameof(Village), nameof(UpdateVillageSpells), fetched));

//            List<Spell> newSpells = new List<Spell>();

//            foreach(Spell spell in fetched.Spells.EmptyIfNull())
//            {
//                Spell? oldSpell = Spells.EmptyIfNull().FirstOrDefault(s => s.Name == spell.Name && s.Village == spell.Village);

//                if (oldSpell == null || oldSpell.Level < spell.Level)
//                {
//                    newSpells.Add(spell);
//                }
//            }

//            if (newSpells.Count > 0)
//            {
//                cocApi.Villages.OnVillageSpellsChanged(fetched, newSpells);
//            }
//        }

//        private void UpdateVillageHeroes(CocApiClient_old cocApi, Village fetched)
//        {
//            cocApi.OnLog(new VillageLogEventArgs(nameof(Village), nameof(UpdateVillageHeroes), fetched));

//            List<Troop> newTroops = new List<Troop>();

//            foreach (Troop troop in fetched.Heroes.EmptyIfNull())
//            {
//                Troop? oldTroop = Heroes.EmptyIfNull().FirstOrDefault(t => t.Name == troop.Name && t.Village == troop.Village);

//                if (oldTroop == null || oldTroop.Level < troop.Level)
//                {
//                    newTroops.Add(troop);
//                }

//            }

//            if (newTroops.Count > 0)
//            {
//                cocApi.Villages.OnVillageHeroesChanged(fetched, newTroops);
//            }
//        }

//        private void UpdateVillageTroops(CocApiClient_old cocApi, Village fetched)
//        {
//            cocApi.OnLog(new VillageLogEventArgs(nameof(Village), nameof(UpdateVillageTroops), fetched));

//            List<Troop> newTroops = new List<Troop>();
            
//            foreach(Troop troop in fetched.Soldiers.EmptyIfNull())
//            {
//                Troop? oldTroop = Soldiers.EmptyIfNull().FirstOrDefault(t => t.Name == troop.Name && t.Village == troop.Village);

//                if (oldTroop == null || oldTroop.Level < troop.Level)
//                {
//                    newTroops.Add(troop);
//                }

//            }

//            if (newTroops.Count > 0)
//            {
//                cocApi.Villages.OnVillageTroopsChanged(fetched, newTroops);
//            }
//        }

//        private void UpdateVillageAchievements(CocApiClient_old cocApi, Village fetched)
//        {
//            cocApi.OnLog(new VillageLogEventArgs(nameof(Village), nameof(UpdateVillageAchievements), fetched));

//            List<Achievement> newAchievements = new List<Achievement>();

//            foreach(Achievement achievement in fetched.Achievements.EmptyIfNull())
//            {
//                if (achievement.Value > achievement.Target)
//                {
//                    Achievement oldAchievement = Achievements.EmptyIfNull().FirstOrDefault(a => a.Name == achievement.Name);

//                    if (oldAchievement == null || oldAchievement.Value < oldAchievement.Target)
//                    {
//                        newAchievements.Add(achievement);
//                    }
//                }
//            }

//            if (newAchievements.Count > 0)
//            {
//                cocApi.Villages.OnVillageAchievementsChanged(fetched, newAchievements);
//            }            
//        }

//        private void UpdateVillage(CocApiClient_old cocApi, Village fetched)
//        {
//            cocApi.OnLog(new VillageLogEventArgs(nameof(Village), nameof(UpdateVillage), fetched));

//            if (cocApi.Villages.IsChanged(fetched, this))
//                cocApi.Villages.OnVillageChanged(fetched, fetched);
//        }

//        public void Initialize(CocApiClient_old cocApi)
//        {
//            VillageTag = VillageTag.ToUpper();

//            if (Clan != null)            
//                ClanTag = Clan.ClanTag;            

//            SetOrderOfHeroes();

//            //todo
//            //SetOrderOfSoldiers();

//            //SetOrderOfSpells();

//            foreach (var hero in Heroes.EmptyIfNull())
//            {
//                Troops ??= new List<Troop>();

//                Troops.Add(hero);

//                hero.IsHero = true;
//            }

//            foreach (var troop in Soldiers.EmptyIfNull())
//            {
//                Troops ??= new List<Troop>();

//                Troops.Add(troop);

//                troop.IsHero = false;
//            }

//            foreach (var achievement in Achievements.EmptyIfNull())
//            {
//                achievement.VillageTag = VillageTag;

//                achievement.Initialize(cocApi);
//            }
//        }

//        //private void SetOrderOfSoldiers()
//        //{
//        //    throw new NotImplementedException();
//        //}

//        //private void SetOrderOfSpells()
//        //{
//        //    foreach (var spell in Spells.EmptyIfNull())
//        //    {
//        //        if (spell.Name == "Barbarian King") spell.Order = 1;
//        //        if (spell.Name == "Archer Queen") spell.Order = 2;
//        //        if (spell.Name == "Grand Warden") spell.Order = 3;
//        //        if (spell.Name == "Royal Champion") spell.Order = 4;
//        //    }

//        //    var i = 5;

//        //    foreach (var spell in Spells.Where(h => h.Order == 0))
//        //    {
//        //        spell.Order = i;

//        //        i++;
//        //    }
//        //}

//        private void SetOrderOfHeroes()
//        {
//            foreach(var hero in Heroes.EmptyIfNull())
//            {
//                if (hero.Name == CocApiClient_old.Heroes.BK) hero.Order = 1;
//                if (hero.Name == CocApiClient_old.Heroes.AQ) hero.Order = 2;
//                if (hero.Name == CocApiClient_old.Heroes.GW) hero.Order = 3;
//                if (hero.Name == CocApiClient_old.Heroes.RC) hero.Order = 4;
//            }

//            var i = 5;

//            foreach(var hero in Heroes.EmptyIfNull().Where(h => h.Order == 0))
//            {
//                hero.Order = i;
//                i++;
//            }
//        }

//        public override string ToString() => $"{VillageTag} {Name} {TownHallLevel}";
//    }
//}
