﻿using System.Collections.Generic;
using System.Text.Json.Serialization;
using CocApiLibrary.Models;

namespace CocApiLibrary
{
    public class VillageAPIModel
    {
        
        private string _tag = string.Empty;
        
        public string Tag
        {
            get
            {
                return _tag;
            }
        
            set
            {
                _tag = value.ToUpper();        	    
            }
        }

        public string Name { get; set; } = string.Empty;

        public int TownHallLevel { get; set; }

        public int TownHallWeaponLevel { get; set; }

        public int ExpLevel { get; set; }

        public int Trophies { get; set; }

        public int BestTrophies { get; set; }

        public int WarStars { get; set; }

        public int AttackWins { get; set; }

        public int DefenseWins { get; set; }

        public int BuilderHallLevel { get; set; }

        public int VersusTrophies { get; set; }

        public int BestVersusTrophies { get; set; }

        public int VersusBattleWins { get; set; }

        public string Role { get; set; } = string.Empty;

        public int Donations { get; set; }

        public int DonationsReceived { get; set; }

        public ClanAPIModel? Clan { get; set; }

        public LeagueAPIModel? League { get; set; }

        public IEnumerable<AchievementAPIModel>? Achievements { get; set; }

        public int VersusBattleWinCount { get; set; }

        public IEnumerable<TroopAPIModel>? Troops { get; set; }

        public IEnumerable<HeroAPIModel>? Heroes { get; set; }

        public IEnumerable<SpellModel>? Spells { get; set; }
    }
}