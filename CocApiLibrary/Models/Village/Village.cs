using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

using devhl.CocApi.Converters;
using devhl.CocApi.Models;
using static devhl.CocApi.Enums;
using devhl.CocApi.Models.Village;

namespace devhl.CocApi.Models.Village
{
    public class Village : Downloadable, IVillage
    {

        [JsonIgnore]
        internal ILogger? Logger { get; set; }

        [JsonProperty("Tag")]
        public string VillageTag { get; private set; } = string.Empty;

        [JsonProperty]
        public string Name { get; } = string.Empty;

        [JsonProperty]
        public string ClanTag { get; private set; } = string.Empty;

        [JsonProperty]
        public int TownHallLevel { get; }

        [JsonProperty]
        public int TownHallWeaponLevel { get; }

        [JsonProperty]
        public int ExpLevel { get; }

        [JsonProperty]
        public int Trophies { get; }

        [JsonProperty]
        public int BestTrophies { get; }

        [JsonProperty]
        public int WarStars { get; }

        [JsonProperty]
        public int AttackWins { get; }

        [JsonProperty]
        public int DefenseWins { get; }

        [JsonProperty]
        public LegendLeagueStatistics? LegendStatistics { get; }

        [JsonProperty]
        public int BuilderHallLevel { get; }

        [JsonProperty]
        public int VersusTrophies { get; }

        [JsonProperty]
        public int BestVersusTrophies { get; }

        [JsonProperty]
        public int VersusBattleWins { get; }

        [JsonProperty]
        public Role Role { get; } = Role.Unknown;

        [JsonProperty]
        public int Donations { get; }

        [JsonProperty]
        public int DonationsReceived { get; }

        [JsonProperty]
        public VillageClan? Clan { get; }

        [JsonProperty]
        public League? League { get; internal set; }

        public int LeagueId { get; private set; }

        [JsonProperty]
        public IEnumerable<Achievement>? Achievements { get; internal set; }


        [JsonProperty]
        public int VersusBattleWinCount { get; }

        [JsonProperty]
        internal IEnumerable<Troop>? Troops { get; set; }

        [JsonProperty]
        internal IEnumerable<Troop>? Heroes { get; set; }

        [JsonProperty]
        public IList<Troop> AllTroops { get; internal set; } = new List<Troop>();

        [JsonProperty]
        public IEnumerable<VillageLabel>? Labels { get; internal set; }

        [JsonProperty]
        public IEnumerable<Spell>? Spells { get; internal set; }




        internal void Update(CocApi cocApi, Village downloadedVillage)
        {
            Logger ??= cocApi.Logger;

            if (ReferenceEquals(this, downloadedVillage))
            {
                return;
            }

            UpdateVillage(cocApi, downloadedVillage);

            UpdateLabels(cocApi, downloadedVillage);

            UpdateVillageDefenseWins(cocApi, downloadedVillage);

            UpdateVillageExpLevel(cocApi, downloadedVillage);

            UpdateVillageTrophies(cocApi, downloadedVillage);

            UpdateVillageVersusBattleWinCount(cocApi, downloadedVillage);

            UpdateVillageVersusBattleWins(cocApi, downloadedVillage);

            UpdateVillageVersusTrophies(cocApi, downloadedVillage);

            UpdateVillageAchievements(cocApi, downloadedVillage);

            UpdateVillageTroops(cocApi, downloadedVillage);

            UpdateVillageHeroes(cocApi, downloadedVillage);

            UpdateVillageSpells(cocApi, downloadedVillage);

            UpdateLegendLeagueStatistics(cocApi, downloadedVillage);
        }

        private void UpdateLegendLeagueStatistics(CocApi cocApi, Village downloadedVillage)
        {
            if (LegendStatistics == null && downloadedVillage.LegendStatistics == null) return;

            if (LegendStatistics == null && downloadedVillage.LegendStatistics != null)
            {
                cocApi.VillageReachedLegendsLeagueEvent(downloadedVillage);
            }
        }

        private void UpdateLabels(CocApi cocApi, Village downloadedVillage)
        {
            List<VillageLabel> added = new List<VillageLabel>();

            List<VillageLabel> removed = new List<VillageLabel>();

            foreach(var newLabel in downloadedVillage.Labels.EmptyIfNull())
            {
                if (!Labels.Any(l => l.Id == newLabel.Id))
                {
                    added.Add(newLabel);
                }
            }

            foreach(var oldLabel in Labels.EmptyIfNull())
            {
                if (!downloadedVillage.Labels.EmptyIfNull().Any(l => l.Id == oldLabel.Id))
                {
                    removed.Add(oldLabel);
                }
            }

            if (Labels == null && downloadedVillage.Labels != null && added.Count == 0)
            {
                foreach (var newLabel in downloadedVillage.Labels)
                {
                    added.Add(newLabel);
                }
            }

            if (downloadedVillage.Labels == null && Labels != null && removed.Count == 0)
            {
                foreach (var removedLabel in Labels)
                {
                    removed.Add(removedLabel);
                }
            }

            cocApi.VillageLabelsChangedEvent(this, added, removed);
        }

        private void UpdateVillageSpells(CocApi cocApi, Village downloadedVillage)
        {
            List<Spell> newSpells = new List<Spell>();

            foreach(Spell spell in downloadedVillage.Spells.EmptyIfNull())
            {
                Spell? oldSpell = Spells.FirstOrDefault(s => s.Name == spell.Name && s.Village == spell.Village);

                if (oldSpell == null || oldSpell.Level < spell.Level)
                {
                    newSpells.Add(spell);
                }
            }

            if (newSpells.Count > 0)
            {
                cocApi.VillageSpellsChangedEvent(this, newSpells);
            }
        }

        private void UpdateVillageHeroes(CocApi cocApi, Village downloadedVillage)
        {
            List<Troop> newTroops = new List<Troop>();

            foreach (Troop troop in downloadedVillage.Heroes.EmptyIfNull())
            {
                Troop? oldTroop = Heroes.FirstOrDefault(t => t.Name == troop.Name && t.Village == troop.Village);

                if (oldTroop == null || oldTroop.Level < troop.Level)
                {
                    newTroops.Add(troop);
                }

            }

            if (newTroops.Count > 0)
            {
                cocApi.VillageHeroesChangedEvent(this, newTroops);
            }
        }

        private void UpdateVillageTroops(CocApi cocApi, Village downloadedVillage)
        {
            List<Troop> newTroops = new List<Troop>();
            
            foreach(Troop troop in downloadedVillage.Troops.EmptyIfNull())
            {
                Troop? oldTroop = Troops.FirstOrDefault(t => t.Name == troop.Name && t.Village == troop.Village);

                if (oldTroop == null || oldTroop.Level < troop.Level)
                {
                    newTroops.Add(troop);
                }

            }

            if (newTroops.Count > 0)
            {
                cocApi.VillageTroopsChangedEvent(this, newTroops);
            }
        }

        private void UpdateVillageAchievements(CocApi cocApi, Village downloadedVillage)
        {
            List<Achievement> newAchievements = new List<Achievement>();

            foreach(Achievement achievement in downloadedVillage.Achievements.EmptyIfNull())
            {
                if (achievement.Value > achievement.Target)
                {
                    Achievement oldAchievement = Achievements.FirstOrDefault(a => a.Name == achievement.Name);

                    if (oldAchievement == null || oldAchievement.Value < oldAchievement.Target)
                    {
                        newAchievements.Add(achievement);
                    }
                }
            }

            if (newAchievements.Count > 0)
            {
                cocApi.VillageAchievementsChangedEvent(this, newAchievements);
            }            
        }

        private void UpdateVillage(CocApi cocApi, Village downloadedVillage)
        {
            if (downloadedVillage.AttackWins != AttackWins ||
                downloadedVillage.BestTrophies != BestTrophies ||
                downloadedVillage.BestVersusTrophies != BestVersusTrophies ||
                downloadedVillage.BuilderHallLevel != BuilderHallLevel ||
                //downloadedVillage.DefenseWins != DefenseWins ||
                //downloadedVillage.Donations != Donations ||
                //downloadedVillage.DonationsReceived != DonationsReceived ||
                //downloadedVillage.ExpLevel != ExpLevel ||
                //downloadedVillage.Name != Name ||
                //downloadedVillage.Role != Role ||
                downloadedVillage.TownHallLevel != TownHallLevel ||
                downloadedVillage.TownHallWeaponLevel != TownHallWeaponLevel ||
                //downloadedVillage.Trophies != Trophies ||
                //downloadedVillage.VersusBattleWinCount != VersusBattleWinCount ||
                //downloadedVillage.VersusBattleWins != VersusBattleWins ||
                //downloadedVillage.VersusTrophies != VersusTrophies ||
                downloadedVillage.WarStars != WarStars
                )
            {
                cocApi.VillageChangedEvent(this, downloadedVillage);

                //AttackWins = downloadedVillage.AttackWins;
                //BestTrophies = downloadedVillage.BestTrophies;
                //BestVersusTrophies = downloadedVillage.BestVersusTrophies;
                //BuilderHallLevel = downloadedVillage.BuilderHallLevel;
                ////Name = downloadedVillage.Name;
                ////Role = downloadedVillage.Role;
                //TownHallLevel = downloadedVillage.TownHallLevel;
                //TownHallWeaponLevel = downloadedVillage.TownHallWeaponLevel;
                //WarStars = downloadedVillage.WarStars;
            }
        }

        private void UpdateVillageDefenseWins(CocApi cocApi, Village downloadedVillage)
        {
            if (downloadedVillage.DefenseWins != DefenseWins)
            {
                cocApi.VillageDefenseWinsChangedEvent(this, downloadedVillage.DefenseWins);
            }
        }

        private void UpdateVillageExpLevel(CocApi cocApi, Village downloadedVillage)
        {
            if (downloadedVillage.ExpLevel != ExpLevel)
            {
                cocApi.VillageExpLevelChangedEvent(this, downloadedVillage.ExpLevel);

                //ExpLevel = downloadedVillage.ExpLevel;
            }
        }

        private void UpdateVillageTrophies(CocApi cocApi, Village downloadedVillage)
        {
            if (downloadedVillage.Trophies != Trophies)
            {
                cocApi.VillageTrophiesChangedEvent(this, downloadedVillage.Trophies);

                //Trophies = downloadedVillage.Trophies;
            }
        }

        private void UpdateVillageVersusBattleWinCount(CocApi cocApi, Village downloadedVillage)
        {
            if (downloadedVillage.VersusBattleWinCount != VersusBattleWinCount)
            {
                cocApi.VillageVersusBattleWinCountChangedEvent(this, downloadedVillage.VersusBattleWinCount);

                //VersusBattleWinCount = downloadedVillage.VersusBattleWinCount;
            }
        }

        private void UpdateVillageVersusBattleWins(CocApi cocApi, Village downloadedVillage)
        {
            if (downloadedVillage.VersusBattleWins != VersusBattleWins)
            {
                cocApi.VillageVersusBattleWinsChangedEvent(this, downloadedVillage.VersusBattleWins);

                //VersusBattleWins = downloadedVillage.VersusBattleWins;
            }
        }

        private void UpdateVillageVersusTrophies(CocApi cocApi, Village downloadedVillage)
        {
            if (downloadedVillage.VersusTrophies != VersusTrophies)
            {
                cocApi.VillageVersusTrophiesChangedEvent(this, downloadedVillage.VersusTrophies);

                //VersusTrophies = downloadedVillage.VersusTrophies;
            }
        }

        public void Initialize()
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

                LegendStatistics.Initialize();

                if (LegendStatistics.BestSeason != null) LegendStatistics.BestSeason.VillageTag = VillageTag;

                if (LegendStatistics.CurrentSeason != null) LegendStatistics.CurrentSeason.VillageTag = VillageTag;

                if (LegendStatistics.PreviousVersusSeason != null) LegendStatistics.PreviousVersusSeason.VillageTag = VillageTag;

                if (LegendStatistics.PreviousVersusSeason != null) LegendStatistics.PreviousVersusSeason.VillageTag = VillageTag;
            }

            foreach (var spell in Spells.EmptyIfNull())
            {
                spell.VillageTag = VillageTag;
            }

            foreach (var hero in Heroes.EmptyIfNull())
            {
                AllTroops.Add(hero);

                hero.VillageTag = VillageTag;

                hero.IsHero = true;
            }

            foreach (var troop in Troops.EmptyIfNull())
            {
                AllTroops.Add(troop);

                troop.VillageTag = VillageTag;

                troop.IsHero = false;
            }

            foreach (var achievement in Achievements.EmptyIfNull())
            {
                achievement.VillageTag = VillageTag;

                achievement.Initialize();
            }

            foreach (var label in Labels.EmptyIfNull())
            {
                label.VillageTag = VillageTag;

                label.Initialize();
            }
        }
    }
}
