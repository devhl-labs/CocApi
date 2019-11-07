using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using CocApiLibrary.Converters;
using CocApiLibrary.Models;
using Microsoft.Extensions.Logging;
using static CocApiLibrary.Enums;

namespace CocApiLibrary
{
    public class VillageApiModel : SwallowDelegates, IVillageApiModel, IDownloadable
    {
        // IVillageApiModel
        [Key]
        [JsonPropertyName("Tag")]
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

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private string _clanTag;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public string ClanTag
        {
            get
            {
                return _clanTag;
            }

            set
            {
                _clanTag = value;

                SetRelationalProperties();
            }
        }




        private string _villageTag = string.Empty;

        public int TownHallLevel { get; set; }

        public int TownHallWeaponLevel { get; set; }

        public int ExpLevel { get; set; }

        public int Trophies { get; set; }

        public int BestTrophies { get; set; }

        public int WarStars { get; set; }

        public int AttackWins { get; set; }

        public int DefenseWins { get; set; }

        [ForeignKey(nameof(VillageTag))]
        public virtual LegendLeagueStatisticsApiModel? LegendStatistics { get; set; }

        public int BuilderHallLevel { get; set; }

        public int VersusTrophies { get; set; }

        public int BestVersusTrophies { get; set; }

        public int VersusBattleWins { get; set; }

        [JsonConverter(typeof(RoleConverter))]
        public Role Role { get; set; } = Role.Unknown;

        public int Donations { get; set; }

        public int DonationsReceived { get; set; }


        private SimpleClanApiModel? _clan;


        [ForeignKey(nameof(ClanTag))]
        [NotMapped]
        public SimpleClanApiModel? Clan
        {
            get
            {
                return _clan;
            }
        
            set
            {
                _clan = value;

                SetRelationalProperties();
            }
        }


        private VillageLeagueApiModel? _league;

        [NotMapped]
        public virtual VillageLeagueApiModel? League
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

        private IEnumerable<AchievementApiModel>? _achievements;

        [ForeignKey(nameof(VillageTag))]
        public virtual IEnumerable<AchievementApiModel>? Achievements
        {
            get
            {
                return _achievements;
            }
        
            set
            {
                _achievements = value;
            }
        }

        public int VersusBattleWinCount { get; set; }


        private IEnumerable<TroopApiModel>? _troops;
        
        [NotMapped]
        public virtual IEnumerable<TroopApiModel>? Troops
        {
            get
            {
                return _troops;
            }
        
            set
            {
                _troops = value;

                //foreach(var troop in Troops.EmptyIfNull())
                //{
                //    AllTroops.Add(troop);
                //}

                //SetRelationalProperties();
            }
        }


        private IEnumerable<TroopApiModel>? _heroes;
        
        [NotMapped]
        public virtual IEnumerable<TroopApiModel>? Heroes
        {
            get
            {
                return _heroes;
            }
        
            set
            {
                _heroes = value;

                //foreach(var hero in Heroes.EmptyIfNull())
                //{
                //    hero.IsHero = true;

                //    AllTroops.Add(hero);
                //}

                //SetRelationalProperties();
            }
        }

        [ForeignKey(nameof(VillageTag))]
        public virtual IList<TroopApiModel> AllTroops { get; set; } = new List<TroopApiModel>();

        private IEnumerable<VillageLabelApiModel>? _labels;

        [ForeignKey("VillageTag")]
        public virtual IEnumerable<VillageLabelApiModel>? Labels
        {
            get
            {
                return _labels;
            }
        
            set
            {
                _labels = value;

                //SetRelationalProperties();
            }
        }


        private IEnumerable<VillageSpellApiModel>? _spells;

        [ForeignKey(nameof(VillageTag))]
        public virtual IEnumerable<VillageSpellApiModel>? Spells
        {
            get
            {
                return _spells;
            }
        
            set
            {
                _spells = value;

                //SetRelationalProperties();
            }
        }

        public DateTime UpdateAtUtc { get; internal set; } = DateTime.UtcNow;

        public DateTime Expires { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;

        public DateTime? CacheExpiresAtUtc { get; set; }

        public bool IsExpired()
        {
            if (DateTime.UtcNow > Expires)
            {
                return true;
            }
            return false;
        }




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

                Swallow(() => UpdateVillage(cocApi, downloadedVillage), nameof(UpdateVillage));

                Swallow(() => UpdateLabels(cocApi, downloadedVillage), nameof(UpdateLabels));

                Swallow(() => UpdateVillageDefenseWins(cocApi, downloadedVillage), nameof(UpdateVillageDefenseWins));

                //UpdateVillageDonations(cocApi, downloadedVillage);

                //UpdateVillageDonationsReceived(cocApi, downloadedVillage);

                Swallow(() => UpdateVillageExpLevel(cocApi, downloadedVillage), nameof(UpdateVillageExpLevel));

                Swallow(() => UpdateVillageTrophies(cocApi, downloadedVillage), nameof(UpdateVillageTrophies));

                Swallow(() => UpdateVillageVersusBattleWinCount(cocApi, downloadedVillage), nameof(UpdateVillageVersusBattleWinCount));

                Swallow(() => UpdateVillageVersusBattleWins(cocApi, downloadedVillage), nameof(UpdateVillageVersusBattleWins));

                Swallow(() => UpdateVillageVersusTrophies(cocApi, downloadedVillage), nameof(UpdateVillageVersusTrophies));

                //UpdateVillageLeague(cocApi, downloadedVillage);

                Swallow(() => UpdateVillageAchievements(cocApi, downloadedVillage), nameof(UpdateVillageAchievements));

                Swallow(() => UpdateVillageTroops(cocApi, downloadedVillage), nameof(UpdateVillageTroops));

                Swallow(() => UpdateVillageHeroes(cocApi, downloadedVillage), nameof(UpdateVillageHeroes));

                Swallow(() => UpdateVillageSpells(cocApi, downloadedVillage), nameof(UpdateVillageSpells));

                Swallow(() => UpdateLegendLeagueStatistics(cocApi, downloadedVillage), nameof(UpdateLegendLeagueStatistics));

                //UpdateAtUtc = downloadedVillage.UpdateAtUtc;

                //Expires = downloadedVillage.Expires;
            }
        }

        private void UpdateLegendLeagueStatistics(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            if (LegendStatistics == null && downloadedVillage.LegendStatistics == null) return;

            if (LegendStatistics == null && downloadedVillage.LegendStatistics != null)
            {
                cocApi.VillageReachedLegendsLeagueEvent(downloadedVillage);
            }

            //LegendStatistics = downloadedVillage.LegendStatistics;
        }

        private void UpdateLabels(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            if (Labels == null && downloadedVillage.Labels == null) return;

            if (Labels != null && Labels.Count() > 0 && (downloadedVillage.Labels == null || downloadedVillage.Labels.Count() == 0))
            {
                cocApi.VillageLabelsRemovedEvent(downloadedVillage, Labels);

                //Labels = downloadedVillage.Labels;
            }
            else if ((Labels == null || Labels.Count() == 0) && downloadedVillage.Labels != null && downloadedVillage.Labels.Count() > 0)
            {
                cocApi.VillageLabelsAddedEvent(downloadedVillage, downloadedVillage.Labels);

                //Labels = downloadedVillage.Labels;
            }
            else
            {
                List<VillageLabelApiModel> added = new List<VillageLabelApiModel>();

                List<VillageLabelApiModel> removed = new List<VillageLabelApiModel>();

                foreach (VillageLabelApiModel labelApiModel in Labels.EmptyIfNull())
                {
                    if (!downloadedVillage.Labels.Any(l => l.Id == labelApiModel.Id))
                    {
                        removed.Add(labelApiModel);
                    }
                }

                foreach (VillageLabelApiModel labelApiModel in downloadedVillage.Labels.EmptyIfNull())
                {
                    if (!Labels.Any(l => l.Id == labelApiModel.Id))
                    {
                        added.Add(labelApiModel);
                    }
                }

                cocApi.VillageLabelsRemovedEvent(downloadedVillage, removed);

                cocApi.VillageLabelsAddedEvent(downloadedVillage, added);

                //Labels = downloadedVillage.Labels;
            }
        }

        private void UpdateVillageSpells(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            List<VillageSpellApiModel> newSpells = new List<VillageSpellApiModel>();

            foreach(VillageSpellApiModel spell in downloadedVillage.Spells.EmptyIfNull())
            {
                VillageSpellApiModel? oldSpell = Spells.FirstOrDefault(s => s.Name == spell.Name);

                if (oldSpell == null || oldSpell.Level < spell.Level)
                {
                    newSpells.Add(spell);
                }
            }

            if (newSpells.Count() > 0)
            {
                cocApi.VillageSpellsChangedEvent(this, newSpells);

                //Spells = downloadedVillage.Spells;
            }
        }

        private void UpdateVillageHeroes(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            List<TroopApiModel> newTroops = new List<TroopApiModel>();

            foreach (TroopApiModel troop in downloadedVillage.Heroes.EmptyIfNull())
            {
                TroopApiModel? oldTroop = Heroes.FirstOrDefault(t => t.Name == troop.Name);

                if (oldTroop == null || oldTroop.Level < troop.Level)
                {
                    newTroops.Add(troop);
                }

            }

            if (newTroops.Count() > 0)
            {
                cocApi.VillageHeroesChangedEvent(this, newTroops);

                //Heroes = downloadedVillage.Heroes;
            }
        }

        private void UpdateVillageTroops(CocApi cocApi, VillageApiModel downloadedVillage)
        {
            List<TroopApiModel> newTroops = new List<TroopApiModel>();
            
            foreach(TroopApiModel troop in downloadedVillage.Troops.EmptyIfNull())
            {
                TroopApiModel? oldTroop = Troops.FirstOrDefault(t => t.Name == troop.Name);

                if (oldTroop == null || oldTroop.Level < troop.Level)
                {
                    newTroops.Add(troop);
                }

            }

            if (newTroops.Count() > 0)
            {
                cocApi.VillageTroopsChangedEvent(this, newTroops);

                //Troops = downloadedVillage.Troops;
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

            if (newAchievements.Count() > 0)
            {
                cocApi.VillageAchievementsChangedEvent(this, newAchievements);

                //Achievements = downloadedVillage.Achievements;
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

                //DefenseWins = downloadedVillage.DefenseWins;
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

        private void SetRelationalProperties()
        {
            if (_clan != null)
            {
                _clanTag = _clan.ClanTag;
            }                
        }
    }
}
