using System;
using System.Collections.Generic;
using System.Linq;
using devhl.CocApi.Exceptions;
using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class Village : Downloadable, IVillage, IInitialize
    {
        public static string Url(string villageTag)
        {
            if (CocApi.TryGetValidTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            return $"https://api.clashofclans.com/v1/players/{Uri.EscapeDataString(formattedTag)}";
        }

        [JsonProperty("Tag")]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        [JsonProperty]
        public string? ClanTag { get; internal set; }


#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        [JsonProperty]
        public int TownHallLevel { get; internal set; }

        [JsonProperty]
        public int TownHallWeaponLevel { get; internal set; }

        [JsonProperty]
        public int ExpLevel { get; internal set; }

        [JsonProperty]
        public int Trophies { get; internal set; }

        [JsonProperty]
        public int BestTrophies { get; internal set; }

        [JsonProperty]
        public int WarStars { get; internal set; }

        [JsonProperty]
        public int AttackWins { get; internal set; }

        [JsonProperty]
        public int DefenseWins { get; internal set; }

        [JsonProperty]
        public LegendLeagueStatistics? LegendStatistics { get; internal set; }

        [JsonProperty]
        public int BuilderHallLevel { get; internal set; }

        [JsonProperty]
        public int VersusTrophies { get; internal set; }

        [JsonProperty]
        public int BestVersusTrophies { get; internal set; }

        [JsonProperty]
        public int VersusBattleWins { get; internal set; }

        [JsonProperty]
        public Role Role { get; internal set; } = Role.Unknown;

        [JsonProperty]
        public int Donations { get; internal set; }

        [JsonProperty]
        public int DonationsReceived { get; internal set; }

        [JsonProperty]
        public VillageClan? Clan { get; private set; }

        [JsonProperty]
        public League? League { get; internal set; }

        public int LeagueId { get; internal set; }

        [JsonProperty]
        public IEnumerable<Achievement>? Achievements { get; internal set; }


        [JsonProperty]
        public int VersusBattleWinCount { get; internal set; }

        [JsonProperty("Troop")]
        internal IEnumerable<Troop>? Soldiers { get; set; }

        [JsonProperty]
        internal IEnumerable<Troop>? Heroes { get; set; }

        [JsonProperty]
        public IList<Troop>? Troops { get; internal set; }

        [JsonProperty]
        public IEnumerable<VillageLabel>? Labels { get; internal set; }

        [JsonProperty]
        public IEnumerable<Spell>? Spells { get; internal set; }




        internal void Update(CocApi cocApi, Village storedVillage)
        {
            if (ReferenceEquals(this, storedVillage)) return;

            UpdateVillage(cocApi, storedVillage);

            UpdateLabels(cocApi, storedVillage);

            UpdateVillageAchievements(cocApi, storedVillage);

            UpdateVillageTroops(cocApi, storedVillage);

            UpdateVillageHeroes(cocApi, storedVillage);

            UpdateVillageSpells(cocApi, storedVillage);

            UpdateLegendLeagueStatistics(cocApi, storedVillage);
        }

        private void UpdateLegendLeagueStatistics(CocApi cocApi, Village downloadedVillage)
        {
            if (LegendStatistics == null && downloadedVillage.LegendStatistics == null) return;

            if (LegendStatistics == null && downloadedVillage.LegendStatistics != null ||
                LegendStatistics?.LegendTrophies != downloadedVillage.LegendStatistics?.LegendTrophies)
            {
                cocApi.Villages.OnVillageLegendLeagueChanged(this, downloadedVillage);

                return;
            }

            if (LegendStatistics?.BestSeason?.Id != downloadedVillage.LegendStatistics?.BestSeason?.Id ||
                LegendStatistics?.BestSeason?.Rank != downloadedVillage.LegendStatistics?.BestSeason?.Rank ||
                LegendStatistics?.BestSeason?.Trophies != downloadedVillage.LegendStatistics?.BestSeason?.Trophies ||

                LegendStatistics?.PreviousVersusSeason?.Id != downloadedVillage.LegendStatistics?.PreviousVersusSeason?.Id ||
                LegendStatistics?.PreviousVersusSeason?.Rank != downloadedVillage.LegendStatistics?.PreviousVersusSeason?.Rank ||
                LegendStatistics?.PreviousVersusSeason?.Trophies != downloadedVillage.LegendStatistics?.PreviousVersusSeason?.Trophies ||

                LegendStatistics?.CurrentSeason?.Id != downloadedVillage.LegendStatistics?.CurrentSeason?.Id ||
                LegendStatistics?.CurrentSeason?.Rank != downloadedVillage.LegendStatistics?.CurrentSeason?.Rank ||
                LegendStatistics?.CurrentSeason?.Trophies != downloadedVillage.LegendStatistics?.CurrentSeason?.Trophies ||

                LegendStatistics?.CurrentVersusSeason?.Id != downloadedVillage.LegendStatistics?.CurrentVersusSeason?.Id ||
                LegendStatistics?.CurrentVersusSeason?.Rank != downloadedVillage.LegendStatistics?.CurrentVersusSeason?.Rank ||
                LegendStatistics?.CurrentVersusSeason?.Trophies != downloadedVillage.LegendStatistics?.CurrentVersusSeason?.Trophies ||

                LegendStatistics?.BestVersusSeason?.Id != downloadedVillage.LegendStatistics?.BestVersusSeason?.Id ||
                LegendStatistics?.BestVersusSeason?.Rank != downloadedVillage.LegendStatistics?.BestVersusSeason?.Rank ||
                LegendStatistics?.BestVersusSeason?.Trophies != downloadedVillage.LegendStatistics?.BestVersusSeason?.Trophies ||

                LegendStatistics?.PreviousSeason?.Id != downloadedVillage.LegendStatistics?.PreviousSeason?.Id ||
                LegendStatistics?.PreviousSeason?.Rank != downloadedVillage.LegendStatistics?.PreviousSeason?.Rank ||
                LegendStatistics?.PreviousSeason?.Trophies != downloadedVillage.LegendStatistics?.PreviousSeason?.Trophies)
            {
                cocApi.Villages.OnVillageLegendLeagueChanged(this, downloadedVillage);
            }
        }

        private void UpdateLabels(CocApi cocApi, Village storedVillage)
        {
            List<VillageLabel> added = new List<VillageLabel>();

            List<VillageLabel> removed = new List<VillageLabel>();

            foreach(var newLabel in Labels.EmptyIfNull())
            {
                if (!storedVillage.Labels.EmptyIfNull().Any(l => l.Id == newLabel.Id))
                {
                    added.Add(newLabel);
                }
            }

            foreach(var oldLabel in storedVillage.Labels.EmptyIfNull())
            {
                if (!Labels.EmptyIfNull().Any(l => l.Id == oldLabel.Id))
                {
                    removed.Add(oldLabel);
                }
            }

            if (storedVillage.Labels == null && Labels != null && added.Count == 0)
            {
                foreach (var newLabel in Labels)
                {
                    added.Add(newLabel);
                }
            }

            if (Labels == null && storedVillage.Labels != null && removed.Count == 0)
            {
                foreach (var removedLabel in storedVillage.Labels)
                {
                    removed.Add(removedLabel);
                }
            }

            cocApi.Villages.OnVillageLabelsChanged(this, added, removed);
        }

        private void UpdateVillageSpells(CocApi cocApi, Village storedVillage)
        {
            List<Spell> newSpells = new List<Spell>();

            foreach(Spell spell in Spells.EmptyIfNull())
            {
                Spell? oldSpell = storedVillage.Spells.EmptyIfNull().FirstOrDefault(s => s.Name == spell.Name && s.Village == spell.Village);

                if (oldSpell == null || oldSpell.Level < spell.Level)
                {
                    newSpells.Add(spell);
                }
            }

            if (newSpells.Count > 0)
            {
                cocApi.Villages.OnVillageSpellsChanged(this, newSpells);
            }
        }

        private void UpdateVillageHeroes(CocApi cocApi, Village storedVillage)
        {
            List<Troop> newTroops = new List<Troop>();

            foreach (Troop troop in Heroes.EmptyIfNull())
            {
                Troop? oldTroop = storedVillage.Heroes.EmptyIfNull().FirstOrDefault(t => t.Name == troop.Name && t.Village == troop.Village);

                if (oldTroop == null || oldTroop.Level < troop.Level)
                {
                    newTroops.Add(troop);
                }

            }

            if (newTroops.Count > 0)
            {
                cocApi.Villages.OnVillageHeroesChanged(this, newTroops);
            }
        }

        private void UpdateVillageTroops(CocApi cocApi, Village storedVillage)
        {
            List<Troop> newTroops = new List<Troop>();
            
            foreach(Troop troop in Soldiers.EmptyIfNull())
            {
                Troop? oldTroop = storedVillage.Soldiers.FirstOrDefault(t => t.Name == troop.Name && t.Village == troop.Village);

                if (oldTroop == null || oldTroop.Level < troop.Level)
                {
                    newTroops.Add(troop);
                }

            }

            if (newTroops.Count > 0)
            {
                cocApi.Villages.OnVillageTroopsChanged(this, newTroops);
            }
        }

        private void UpdateVillageAchievements(CocApi cocApi, Village storedVillage)
        {
            List<Achievement> newAchievements = new List<Achievement>();

            foreach(Achievement achievement in Achievements.EmptyIfNull())
            {
                if (achievement.Value > achievement.Target)
                {
                    Achievement oldAchievement = storedVillage.Achievements.EmptyIfNull().FirstOrDefault(a => a.Name == achievement.Name);

                    if (oldAchievement == null || oldAchievement.Value < oldAchievement.Target)
                    {
                        newAchievements.Add(achievement);
                    }
                }
            }

            if (newAchievements.Count > 0)
            {
                cocApi.Villages.OnVillageAchievementsChanged(this, newAchievements);
            }            
        }

        private void UpdateVillage(CocApi cocApi, Village storedVillage)
        {
            if (storedVillage.AttackWins != AttackWins ||
                storedVillage.BestTrophies != BestTrophies ||
                storedVillage.BestVersusTrophies != BestVersusTrophies ||
                storedVillage.BuilderHallLevel != BuilderHallLevel ||
                storedVillage.TownHallLevel != TownHallLevel ||
                storedVillage.TownHallWeaponLevel != TownHallWeaponLevel ||
                storedVillage.WarStars != WarStars || 
                storedVillage.DefenseWins != DefenseWins ||
                storedVillage.ExpLevel != ExpLevel ||
                storedVillage.Trophies != Trophies ||
                storedVillage.VersusBattleWinCount != VersusBattleWinCount ||
                storedVillage.VersusBattleWins != VersusBattleWins ||
                storedVillage.VersusTrophies != VersusTrophies ||
                storedVillage.ClanTag != ClanTag)
            {
                cocApi.Villages.OnVillageChanged(this, storedVillage);
            }
        }

        public void Initialize(CocApi cocApi)
        {
            VillageTag = VillageTag.ToUpper();

            if (Clan != null)
            {
                ClanTag = Clan.ClanTag;
            }

            if (League != null)
            {
                LeagueId = League.Id;
            }

            if (LegendStatistics != null)
            {
                LegendStatistics.VillageTag = VillageTag;

                LegendStatistics.Initialize(cocApi);

                if (LegendStatistics.BestSeason != null) LegendStatistics.BestSeason.VillageTag = VillageTag;

                if (LegendStatistics.CurrentSeason != null) LegendStatistics.CurrentSeason.VillageTag = VillageTag;

                if (LegendStatistics.PreviousVersusSeason != null) LegendStatistics.PreviousVersusSeason.VillageTag = VillageTag;

                if (LegendStatistics.PreviousVersusSeason != null) LegendStatistics.PreviousVersusSeason.VillageTag = VillageTag;
            }

            foreach (var spell in Spells.EmptyIfNull())
            {
                spell.VillageTag = VillageTag;
            }

            SetOrderOfHeroes();

            //todo
            //SetOrderOfSoldiers();

            //SetOrderOfSpells();

            foreach (var hero in Heroes.EmptyIfNull())
            {
                Troops ??= new List<Troop>();

                Troops.Add(hero);

                hero.VillageTag = VillageTag;

                hero.IsHero = true;
            }

            foreach (var troop in Soldiers.EmptyIfNull())
            {
                Troops ??= new List<Troop>();

                Troops.Add(troop);

                troop.VillageTag = VillageTag;

                troop.IsHero = false;
            }

            foreach (var achievement in Achievements.EmptyIfNull())
            {
                achievement.VillageTag = VillageTag;

                achievement.Initialize(cocApi);
            }

            foreach (var label in Labels.EmptyIfNull())
            {
                label.VillageTag = VillageTag;

                label.Initialize(cocApi);
            }
        }

        //private void SetOrderOfSoldiers()
        //{
        //    throw new NotImplementedException();
        //}

        //private void SetOrderOfSpells()
        //{
        //    foreach (var spell in Spells.EmptyIfNull())
        //    {
        //        if (spell.Name == "Barbarian King") spell.Order = 1;
        //        if (spell.Name == "Archer Queen") spell.Order = 2;
        //        if (spell.Name == "Grand Warden") spell.Order = 3;
        //        if (spell.Name == "Royal Champion") spell.Order = 4;
        //    }

        //    var i = 5;

        //    foreach (var spell in Spells.Where(h => h.Order == 0))
        //    {
        //        spell.Order = i;

        //        i++;
        //    }
        //}

        private void SetOrderOfHeroes()
        {
            foreach(var hero in Heroes.EmptyIfNull())
            {
                if (hero.Name == CocApi.Heroes.BK) hero.Order = 1;
                if (hero.Name == CocApi.Heroes.AQ) hero.Order = 2;
                if (hero.Name == CocApi.Heroes.GW) hero.Order = 3;
                if (hero.Name == CocApi.Heroes.RC) hero.Order = 4;
            }

            var i = 5;

            foreach(var hero in Heroes.Where(h => h.Order == 0))
            {
                hero.Order = i;
                i++;
            }
        }

        public override string ToString() => $"{VillageTag} {Name} {TownHallLevel}";
    }
}
