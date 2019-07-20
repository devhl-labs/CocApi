using CocApiLibrary.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class CurrentWarAPIModel : IProcess
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
                if(Clan != null)
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
                if(Opponent != null)
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

        [JsonIgnore]
        public Result Result { get; set; }  //todo update this when appropriate






        void IProcess.Process()
        {
            Clans = Clans.OrderBy(x => x.Tag).ToList();

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

                            MemberAPIModel defendingBase =(MemberAPIModel) Clans.First(x => x.Tag != clan.Tag).Members.First(x => x.Tag == attack.DefenderTag);

                            defendingBase.Defenses.Add(attack);

                            clan.AttacksList.Add(attack);

                            Clans.First(x => x.Tag != clan.Tag).DefensesList.Add(attack);
                        }
                    }
                }
            }

            Attacks = Attacks.OrderBy(x => x.Order).ToList();

            foreach (WarClanAPIModel clan in Clans)
            {
                clan.AttacksList = clan.AttacksList.OrderBy(x => x.Order).ToList();
                clan.DefensesList = clan.DefensesList.OrderBy(x => x.Order).ToList();

                clan.DefenseCount = clan.DefensesList.Count();

                if (clan.Members == null)
                {
                    continue;
                }

                foreach (MemberAPIModel member in clan.Members)
                {
                    member.Attacks = member.Attacks.OrderBy(x => x.Order).ToList();
                    member.Defenses = member.Defenses.OrderBy(x => x.Order).ToList();
                }
            }
        }
    }
}
