using CocApiLibrary.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class CurrentWarAPIModel
    {
        [JsonPropertyName("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUTC { get; set; }

        [JsonPropertyName("preparationStartTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime PreparationStartTimeUTC { get; set; }

        [JsonPropertyName("startTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime StartTimeUTC { get; set; }

        public int TeamSize { get; set; }

        private WarClanAPIModel? _clan;

        public WarClanAPIModel? Clan
        {
            get { return _clan; }
            set {
                _clan = value;
                if (Clan != null)
                {
                    Clans.Add(Clan);
                }
            }
        }

        private WarClanAPIModel? _opponent;

        public WarClanAPIModel? Opponent
        {
            get { return _opponent; }
            set {
                _opponent = value;
                if (Opponent != null)
                {
                    Clans.Add(Opponent);
                }
            }
        }

        public State State { get; set; }







        [JsonIgnore]
        public IList<WarClanAPIModel> Clans { get; set; } = new List<WarClanAPIModel>();

        [JsonIgnore]
        public IList<AttackAPIModel> Attacks { get; set; } = new List<AttackAPIModel>();

        /// <summary>
        /// This amalgamation is a composite key of the preparation start time and clan tags.
        /// </summary>
        [JsonIgnore]
        public string WarID { get; set; } = string.Empty;

        [JsonIgnore]
        public WarType WarType { get; set; } = WarType.Random;







        internal void Process()
        {
            Clans = Clans.OrderBy(x => x.Tag).ToList();

            WarID = $"{PreparationStartTimeUTC};{Clans[0].Tag};{Clans[1].Tag}";

            TimeSpan timeSpan = StartTimeUTC - PreparationStartTimeUTC;

            if (timeSpan.TotalHours == 24
                || timeSpan.TotalHours == 20
                || timeSpan.TotalHours == 16
                || timeSpan.TotalHours == 12
                || timeSpan.TotalHours == 8
                || timeSpan.TotalHours == 6
                || timeSpan.TotalHours == 4
                || timeSpan.TotalHours == 2
                || timeSpan.TotalHours == 1
                || timeSpan.TotalMinutes == 30
                || timeSpan.TotalMinutes == 15)
            {
                WarType = WarType.Friendly;
            }

            if (WarIsOverOrAllAttacksUsed())
            {
                if (Clans[0].Stars == Clans[1].Stars)
                {
                    if(Clans[0].DestructionPercentage == Clans[1].DestructionPercentage)
                    {
                        Clans[0].Result = Result.Draw;
                        Clans[1].Result = Result.Draw;
                    }
                    else if(Clans[0].DestructionPercentage > Clans[1].DestructionPercentage)
                    {
                        Clans[0].Result = Result.Win;
                        Clans[1].Result = Result.Lose;
                    }
                    else
                    {
                        Clans[0].Result = Result.Lose;
                        Clans[1].Result = Result.Win;
                    }
                }
                else if(Clans[0].Stars > Clans[1].Stars)
                {
                    Clans[0].Result = Result.Win;
                    Clans[1].Result = Result.Lose;
                }
                else
                {
                    Clans[0].Result = Result.Lose;
                    Clans[1].Result = Result.Win;
                }
            }

            foreach (WarClanAPIModel clan in Clans)
            {
                if (clan.Members == null)
                {
                    continue;
                }
                foreach (MemberAPIModel member in clan.Members)
                {
                    if (member.Attacks != null)
                    {
                        foreach (AttackAPIModel attack in member.Attacks)
                        {
                            Attacks.Add(attack);

                            MemberAPIModel defendingBase = Clans.First(x => x.Tag != clan.Tag).Members.First(x => x.Tag == attack.DefenderTag);

                            defendingBase.Defenses.Add(attack);

                            clan.Attacks.Add(attack);

                            Clans.First(x => x.Tag != clan.Tag).Defenses.Add(attack);
                        }
                    }
                }
            }

            Attacks = Attacks.OrderBy(x => x.Order).ToList();

            foreach (WarClanAPIModel clan in Clans)
            {
                clan.Attacks = clan.Attacks.OrderBy(x => x.Order).ToList();
                clan.Defenses = clan.Defenses.OrderBy(x => x.Order).ToList();

                clan.DefenseCount = clan.Defenses.Count();

                if (clan.Members == null)
                {
                    continue;
                }

                foreach (MemberAPIModel member in clan.Members)
                {
                    member.Attacks = member.Attacks?.OrderBy(x => x.Order).ToList();
                    member.Defenses = member.Defenses.OrderBy(x => x.Order).ToList();
                }
            }
        }

        public bool WarIsOverOrAllAttacksUsed()
        { 
            if (State == State.WarEnded) return true;

            if (Clans[0].Members.All(m => m.Attacks?.Count() == 2) && Clans[1].Members.All(m => m.Attacks?.Count() == 2)) return true;

            return false;
        }
    }
}
