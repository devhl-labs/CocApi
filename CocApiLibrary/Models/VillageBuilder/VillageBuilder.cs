using System;
using System.Collections.Generic;
using System.Linq;
using devhl.CocApi.Exceptions;
using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class VillageBuilder
    {
        public string VillageTag { get; set; } = string.Empty;


        public string Name { get; set; } = string.Empty;

#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.


        public string? ClanTag { get; set; }


#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.


        public int TownHallLevel { get; set; }


        public int TownHallWeaponLevel { get; set; }


        public int ExpLevel { get; set; }


        public int Trophies { get; set; }


        public int BestTrophies { get; }


        public int WarStars { get; set; }


        public int AttackWins { get; set; }


        public int DefenseWins { get; set; }


        public LegendLeagueStatisticsBuilder? LegendStatistics { get; set; }


        public int BuilderHallLevel { get; set; }


        public int VersusTrophies { get; set; }


        public int BestVersusTrophies { get; set; }


        public int VersusBattleWins { get; set; }


        public Role Role { get; set; } = Role.Unknown;


        public int Donations { get; set; }


        public int DonationsReceived { get; set; }


        //public VillageClan? Clan { get; set; }


        //public League? League { get; set; }

        public int LeagueId { get; set; }

        public IEnumerable<AchievementBuilder>? Achievements { get; set; }

        public int VersusBattleWinCount { get; set; }

        //internal IEnumerable<Troop>? Soldiers { get; set; }

        //internal IEnumerable<Troop>? Heroes { get; set; }

        public IList<TroopBuilder>? Troops { get; set; }

        public IEnumerable<VillageLabelBuilder>? Labels { get; set; }

        public IEnumerable<SpellBuilder>? Spells { get; set; }

        public override string ToString() => $"{VillageTag} {Name} {TownHallLevel}";

        public Village Build()
        {
            Village village = new Village
            {
                VillageTag = VillageTag,
                Name = Name,
                ClanTag = ClanTag,
                TownHallLevel = TownHallLevel,
                TownHallWeaponLevel = TownHallWeaponLevel,
                ExpLevel = ExpLevel,
                Trophies = Trophies,
                BestTrophies = BestTrophies,
                WarStars = WarStars,
                AttackWins = AttackWins,
                DefenseWins = DefenseWins,
                BuilderHallLevel = BuilderHallLevel,
                VersusTrophies = VersusTrophies,
                BestVersusTrophies = BestVersusTrophies,
                VersusBattleWins = VersusBattleWins,
                Role = Role,
                Donations = Donations,
                DonationsReceived = DonationsReceived,
                LeagueId = LeagueId,
                VersusBattleWinCount = VersusBattleWinCount,
                LegendStatistics = LegendStatistics?.Build()
            };

            if (Achievements != null)
            {
                List<Achievement> achievements = new List<Achievement>();
                foreach (AchievementBuilder achievement in Achievements)
                    achievements.Add(achievement.Build());
                village.Achievements = achievements;
            }

            if (Troops != null)
            {
                List<Troop> troops = new List<Troop>();
                foreach (TroopBuilder troop in Troops)
                    troops.Add(troop.Build());
                village.Troops = troops;

                List<Troop> heroes = new List<Troop>();
                foreach(TroopBuilder hero in Troops.Where(t => t.IsHero))
                    heroes.Add(hero.Build());
                village.Heroes = heroes;

                List<Troop> soldiers = new List<Troop>();
                foreach(TroopBuilder soldier in Troops.Where(t => t.IsHero == false))
                    soldiers.Add(soldier.Build());
                village.Soldiers = soldiers;
                
            }

            if (Labels != null)
            {
                List<VillageLabel> labels = new List<VillageLabel>();
                foreach (VillageLabelBuilder villageLabelBuilder in Labels)
                    labels.Add(villageLabelBuilder.Build());
                village.Labels = labels;
            }           
            
            if (Spells != null)
            {
                List<Spell> spells = new List<Spell>();
                foreach (SpellBuilder spellBuilder in Spells)
                    spells.Add(spellBuilder.Build());
                village.Spells = spells;
            }

            return village;
        }
    }
}
