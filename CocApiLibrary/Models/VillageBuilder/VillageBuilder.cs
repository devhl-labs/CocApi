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
                village.Achievements = new List<Achievement>();
                foreach (AchievementBuilder achievement in Achievements)
                    village.Achievements.Append(achievement.Build());
            }

            if (Troops != null)
            {
                village.Troops = new List<Troop>();
                foreach (TroopBuilder troop in Troops)
                    village.Troops.Add(troop.Build());

                foreach(TroopBuilder hero in Troops.Where(t => t.IsHero))
                {
                    village.Heroes ??= new List<Troop>();
                    village.Heroes.Append(hero.Build());
                }

                foreach(TroopBuilder soldier in Troops.Where(t => t.IsHero == false))
                {
                    village.Soldiers ??= new List<Troop>();
                    village.Soldiers.Append(soldier.Build());
                }
            }

            if (Labels != null)
            {
                village.Labels = new List<VillageLabel>();
                foreach (VillageLabelBuilder villageLabelBuilder in Labels)
                    village.Labels.Append(villageLabelBuilder.Build());
            }           
            
            if (Spells != null)
            {
                village.Spells = new List<Spell>();
                foreach (SpellBuilder spellBuilder in Spells)
                    village.Spells.Append(spellBuilder.Build());
            }

            return village;
        }
    }
}
