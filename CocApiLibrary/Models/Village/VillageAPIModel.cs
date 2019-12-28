using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
////System.Text.Json.Serialization
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

using devhl.CocApi.Converters;
using devhl.CocApi.Models;
using static devhl.CocApi.Enums;
using devhl.CocApi.Models.Village;

namespace devhl.CocApi.Models.Village
{
    public class VillageApiModel : Downloadable, IVillageApiModel, IInitialize
    {

        [JsonIgnore]
        public ILogger? Logger { get; set; }



        // IVillageApiModel

        [JsonProperty("Tag")]
        public string VillageTag
        {
            get
            {
                return _villageTag;
            }

            set
            {
                _villageTag = value.ToUpper();
            }
        }

        public string Name { get; set; } = string.Empty;

        public string ClanTag { get; set; } = string.Empty;




        private string _villageTag = string.Empty;

        public int TownHallLevel { get; set; }

        public int TownHallWeaponLevel { get; set; }

        public int ExpLevel { get; set; }

        public int Trophies { get; set; }

        public int BestTrophies { get; set; }

        public int WarStars { get; set; }

        public int AttackWins { get; set; }

        public int DefenseWins { get; set; }

        public LegendLeagueStatisticsApiModel? LegendStatistics { get; set; }

        public int BuilderHallLevel { get; set; }

        public int VersusTrophies { get; set; }

        public int BestVersusTrophies { get; set; }

        public int VersusBattleWins { get; set; }


        public Role Role { get; set; } = Role.Unknown;

        public int Donations { get; set; }

        public int DonationsReceived { get; set; }




        public VillageClanApiModel? Clan { get; set; }


        private VillageLeagueApiModel? _league;


        public VillageLeagueApiModel? League
        {
            get
            {
                return _league;
            }
        
            set
            {
                _league = value;

                if (_league != null)
                {
                    LeagueId = _league.Id;
                }
            }
        }

        public int LeagueId { get; set; }


        public IEnumerable<AchievementApiModel>? Achievements { get; set; }

        public int VersusBattleWinCount { get; set; }

        

        public IEnumerable<TroopApiModel>? Troops { get; set; }



        public IEnumerable<TroopApiModel>? Heroes { get; set; }


        public IList<TroopApiModel> AllTroops { get; set; } = new List<TroopApiModel>();


        public IEnumerable<VillageLabelApiModel>? Labels { get; set; }


        public IEnumerable<VillageSpellApiModel>? Spells { get; set; }




        private readonly object _updateLock = new object();

        internal void Update(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            lock (_updateLock)
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

                //UpdateLeague(cocApi, downloadedVillage);
            }
        }

        private void UpdateLegendLeagueStatistics(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            if (LegendStatistics == null && downloadedVillage.LegendStatistics == null) return;

            if (LegendStatistics == null && downloadedVillage.LegendStatistics != null)
            {
                cocApi.VillageReachedLegendsLeagueEvent(downloadedVillage);
            }
        }

        private void UpdateLabels(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            List<VillageLabelApiModel> added = new List<VillageLabelApiModel>();

            List<VillageLabelApiModel> removed = new List<VillageLabelApiModel>();

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

        private void UpdateVillageSpells(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            List<VillageSpellApiModel> newSpells = new List<VillageSpellApiModel>();

            foreach(VillageSpellApiModel spell in downloadedVillage.Spells.EmptyIfNull())
            {
                VillageSpellApiModel? oldSpell = Spells.FirstOrDefault(s => s.Name == spell.Name && s.Village == spell.Village);

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

        private void UpdateVillageHeroes(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            List<TroopApiModel> newTroops = new List<TroopApiModel>();

            foreach (TroopApiModel troop in downloadedVillage.Heroes.EmptyIfNull())
            {
                TroopApiModel? oldTroop = Heroes.FirstOrDefault(t => t.Name == troop.Name && t.Village == troop.Village);

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

        private void UpdateVillageTroops(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            List<TroopApiModel> newTroops = new List<TroopApiModel>();
            
            foreach(TroopApiModel troop in downloadedVillage.Troops.EmptyIfNull())
            {
                TroopApiModel? oldTroop = Troops.FirstOrDefault(t => t.Name == troop.Name && t.Village == troop.Village);

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

        private void UpdateVillageAchievements(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            List<AchievementApiModel> newAchievements = new List<AchievementApiModel>();

            foreach(AchievementApiModel achievement in downloadedVillage.Achievements.EmptyIfNull())
            {
                if (achievement.Value > achievement.Target)
                {
                    AchievementApiModel oldAchievement = Achievements.FirstOrDefault(a => a.Name == achievement.Name);

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

        private void UpdateVillage(CocApi cocApi, VillageApiModel downloadedVillage)
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

        private void UpdateVillageDefenseWins(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            if (downloadedVillage.DefenseWins != DefenseWins)
            {
                cocApi.VillageDefenseWinsChangedEvent(this, downloadedVillage.DefenseWins);
            }
        }

        private void UpdateVillageExpLevel(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            if (downloadedVillage.ExpLevel != ExpLevel)
            {
                cocApi.VillageExpLevelChangedEvent(this, downloadedVillage.ExpLevel);

                //ExpLevel = downloadedVillage.ExpLevel;
            }
        }

        private void UpdateVillageTrophies(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            if (downloadedVillage.Trophies != Trophies)
            {
                cocApi.VillageTrophiesChangedEvent(this, downloadedVillage.Trophies);

                //Trophies = downloadedVillage.Trophies;
            }
        }

        private void UpdateVillageVersusBattleWinCount(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            if (downloadedVillage.VersusBattleWinCount != VersusBattleWinCount)
            {
                cocApi.VillageVersusBattleWinCountChangedEvent(this, downloadedVillage.VersusBattleWinCount);

                //VersusBattleWinCount = downloadedVillage.VersusBattleWinCount;
            }
        }

        private void UpdateVillageVersusBattleWins(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            if (downloadedVillage.VersusBattleWins != VersusBattleWins)
            {
                cocApi.VillageVersusBattleWinsChangedEvent(this, downloadedVillage.VersusBattleWins);

                //VersusBattleWins = downloadedVillage.VersusBattleWins;
            }
        }

        private void UpdateVillageVersusTrophies(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            if (downloadedVillage.VersusTrophies != VersusTrophies)
            {
                cocApi.VillageVersusTrophiesChangedEvent(this, downloadedVillage.VersusTrophies);

                //VersusTrophies = downloadedVillage.VersusTrophies;
            }
        }

        public void Initialize()
        {
            if (Clan != null)
            {
                ClanTag = Clan.ClanTag;
            }

            if (LegendStatistics != null)
            {
                LegendStatistics.VillageTag = VillageTag;

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
            }

            foreach (var label in Labels.EmptyIfNull())
            {
                label.VillageTag = VillageTag;
            }
        }
    }
}
