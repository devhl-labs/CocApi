using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class CurrentWarAPIModel
    {
        private string _endTime = string.Empty;

        private string _preparationStartTime = string.Empty;

        private string _startTime = string.Empty;
        public string EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;

                EndTimeUTC = _endTime.ToDateTime();
            }
        }




        public DateTime EndTimeUTC { get; set; }

        public string PreparationStartTime
        {
            get { return _preparationStartTime; }
            set
            {
                _preparationStartTime = value;

                PreparationStartTimeUTC = _preparationStartTime.ToDateTime();
            }
        }

        public DateTime PreparationStartTimeUTC { get; set; }

        public string StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;

                StartTimeUTC = _startTime.ToDateTime();
            }
        }

        public DateTime StartTimeUTC { get; set; }

        private string _stateString = string.Empty;

        [JsonPropertyName("State")]
        public string StateString
        {
            get { return _stateString; }
            set {
                _stateString = value;
                if(Enum.TryParse(_stateString, out State state))
                {
                    StateEnum = state;
                }
            }
        }

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



        [JsonIgnore]
        public IList<WarClanAPIModel> Clans { get; set; } = new List<WarClanAPIModel>();

        [JsonIgnore]
        public IList<AttackAPIModel> AttackList { get; set; } = new List<AttackAPIModel>();

        [JsonIgnore]
        public State StateEnum { get; set; }






        internal void Process()
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
                    member.ClanTag = clan.Tag;

                    if (AttackList != null)
                    {
                        foreach (AttackAPIModel attack in member.Attacks)
                        {
                            AttackList.Add(attack);

                            MemberAPIModel defendingBase = Clans.First(x => x.Tag != clan.Tag).Members.First(x => x.Tag == attack.DefenderTag);

                            defendingBase.Defenses.Add(attack);

                            clan.AttacksList.Add(attack);

                            Clans.First(x => x.Tag != clan.Tag).DefensesList.Add(attack);
                        }
                    }
                }
            }

            AttackList = AttackList.OrderBy(x => x.Order).ToList();

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
                    member.ClanTag = clan.Tag;

                    member.Attacks = member.Attacks.OrderBy(x => x.Order).ToList();
                    member.Defenses = member.Defenses.OrderBy(x => x.Order).ToList();
                }
            }
        }
    }
}
