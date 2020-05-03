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

        //[JsonIgnore]
        //internal ILogger? Logger { get; set; }

        [JsonProperty("Tag")]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;

#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        [JsonProperty]
        public string? ClanTag { get; internal set; }


#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        [JsonProperty]
        public int TownHallLevel { get; private set; }

        [JsonProperty]
        public int TownHallWeaponLevel { get; private set; }

        [JsonProperty]
        public int ExpLevel { get; private set; }

        [JsonProperty]
        public int Trophies { get; private set; }

        [JsonProperty]
        public int BestTrophies { get; }

        [JsonProperty]
        public int WarStars { get; private set; }

        [JsonProperty]
        public int AttackWins { get; private set; }

        [JsonProperty]
        public int DefenseWins { get; private set; }

        [JsonProperty]
        public LegendLeagueStatistics? LegendStatistics { get; private set; }

        [JsonProperty]
        public int BuilderHallLevel { get; private set; }

        [JsonProperty]
        public int VersusTrophies { get; private set; }

        [JsonProperty]
        public int BestVersusTrophies { get; private set; }

        [JsonProperty]
        public int VersusBattleWins { get; private set; }

        [JsonProperty]
        public Role Role { get; private set; } = Role.Unknown;

        [JsonProperty]
        public int Donations { get; private set; }

        [JsonProperty]
        public int DonationsReceived { get; private set; }

        [JsonProperty]
        public VillageClan? Clan { get; private set; }

        [JsonProperty]
        public League? League { get; internal set; }

        public int LeagueId { get; private set; }

        [JsonProperty]
        public IEnumerable<Achievement>? Achievements { get; internal set; }


        [JsonProperty]
        public int VersusBattleWinCount { get; private set; }

        [JsonProperty("Troop")]
        internal IEnumerable<Troop>? Soldiers { get; set; }

        [JsonProperty]
        internal IEnumerable<Troop>? Heroes { get; set; }

        [JsonProperty]
        public IList<Troop> Troops { get; internal set; } = new List<Troop>();

        [JsonProperty]
        public IEnumerable<VillageLabel>? Labels { get; internal set; }

        [JsonProperty]
        public IEnumerable<Spell>? Spells { get; internal set; }




        internal void Update(CocApi cocApi, Village storedVillage)
        {
            if (ReferenceEquals(this, storedVillage)) return;

            UpdateVillage(cocApi, storedVillage);

            UpdateClan(cocApi, storedVillage);

            UpdateLabels(cocApi, storedVillage);

            UpdateVillageDefenseWins(cocApi, storedVillage);

            UpdateVillageExpLevel(cocApi, storedVillage);

            UpdateVillageTrophies(cocApi, storedVillage);

            UpdateVillageVersusBattleWinCount(cocApi, storedVillage);

            UpdateVillageVersusBattleWins(cocApi, storedVillage);

            UpdateVillageVersusTrophies(cocApi, storedVillage);

            UpdateVillageAchievements(cocApi, storedVillage);

            UpdateVillageTroops(cocApi, storedVillage);

            UpdateVillageHeroes(cocApi, storedVillage);

            UpdateVillageSpells(cocApi, storedVillage);

            UpdateLegendLeagueStatistics(cocApi, storedVillage);
        }

        private void UpdateLegendLeagueStatistics(CocApi cocApi, Village downloadedVillage)
        {
            if (LegendStatistics == null && downloadedVillage.LegendStatistics == null) return;

            if (LegendStatistics == null && downloadedVillage.LegendStatistics != null)
            {
                cocApi.Villages.VillageReachedLegendsLeagueEvent(downloadedVillage);
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

            cocApi.Villages.VillageLabelsChangedEvent(this, added, removed);
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
                cocApi.Villages.VillageSpellsChangedEvent(this, newSpells);
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
                cocApi.Villages.VillageHeroesChangedEvent(this, newTroops);
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
                cocApi.Villages.VillageTroopsChangedEvent(this, newTroops);
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
                storedVillage.WarStars != WarStars
                )
            {
                cocApi.Villages.VillageChangedEvent(storedVillage, this);
            }
        }

        private void UpdateClan(CocApi cocApi, Village storedVillage)
        {
            if (ClanTag != storedVillage.ClanTag)
                cocApi.Villages.OnClanChanged(this, storedVillage);
        }

        private void UpdateVillageDefenseWins(CocApi cocApi, Village storedVillage)
        {
            if (storedVillage.DefenseWins != DefenseWins)
            {
                cocApi.Villages.VillageDefenseWinsChangedEvent(this, DefenseWins - storedVillage.DefenseWins);
            }
        }

        private void UpdateVillageExpLevel(CocApi cocApi, Village storedVillage)
        {
            if (storedVillage.ExpLevel != ExpLevel)
            {
                cocApi.Villages.VillageExpLevelChangedEvent(this, ExpLevel - storedVillage.ExpLevel);
            }
        }

        private void UpdateVillageTrophies(CocApi cocApi, Village storedVillage)
        {
            if (storedVillage.Trophies != Trophies)
            {
                cocApi.Villages.VillageTrophiesChangedEvent(this, Trophies - storedVillage.Trophies);
            }
        }

        private void UpdateVillageVersusBattleWinCount(CocApi cocApi, Village storedVillage)
        {
            if (storedVillage.VersusBattleWinCount != VersusBattleWinCount)
            {
                cocApi.Villages.VillageVersusBattleWinCountChangedEvent(this, VersusBattleWinCount - storedVillage.VersusBattleWinCount);
            }
        }

        private void UpdateVillageVersusBattleWins(CocApi cocApi, Village storedVillage)
        {
            if (storedVillage.VersusBattleWins != VersusBattleWins)
            {
                cocApi.Villages.VillageVersusBattleWinsChangedEvent(this, VersusBattleWins - storedVillage.VersusBattleWins);
            }
        }

        private void UpdateVillageVersusTrophies(CocApi cocApi, Village storedVillage)
        {
            if (storedVillage.VersusTrophies != VersusTrophies)
            {
                cocApi.Villages.VillageVersusTrophiesChangedEvent(this, VersusTrophies - storedVillage.VersusTrophies);
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
                Troops.Add(hero);

                hero.VillageTag = VillageTag;

                hero.IsHero = true;
            }

            foreach (var troop in Soldiers.EmptyIfNull())
            {
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
                if (hero.Name == "Barbarian King") hero.Order = 1;
                if (hero.Name == "Archer Queen") hero.Order = 2;
                if (hero.Name == "Grand Warden") hero.Order = 3;
                if (hero.Name == "Royal Champion") hero.Order = 4;
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
