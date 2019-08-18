using CocApiLibrary.Converters;
using CocApiLibrary.Models.Clan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class CurrentWarAPIModel : IDownloadable, IInitialize, ICurrentWarAPIModel
    {
        //private CocApi? _cocApi;
        //private bool _changed = false;

        //public void Process(CocApi cocApi)
        //{
        //    _cocApi = cocApi;
        //}

        //internal void FireEvent()
        //{
        //    if (_changed && _cocApi != null)
        //    {
        //        _changed = false;
        //        //_cocApi.CurrentWarChangedEvent(this);
        //    }
        //}



        private DateTime _endTimeUTC;

        [JsonPropertyName("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUTC
        {
            get
            {
                return _endTimeUTC;
            }

            set
            {
                if (_endTimeUTC != value)
                {
                    _endTimeUTC = value;

                    WarEndingSoonUTC = _endTimeUTC.AddHours(-1);

                    if (DateTime.UtcNow > WarEndingSoonUTC)
                    {
                        Flags.WarEndingSoon = true;
                    }

                    //if(_cocApi != null)
                    //{
                    // _changed = true;
                    //}
                }
            }
        }

        [JsonPropertyName("preparationStartTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime PreparationStartTimeUTC { get; set; }


        private DateTime _startTimeUTC;

        [JsonPropertyName("startTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime StartTimeUTC
        {
            get
            {
                return _startTimeUTC;
            }

            set
            {
                if (_startTimeUTC != value)
                {
                    _startTimeUTC = value;

                    WarStartingSoonUTC = _startTimeUTC.AddHours(-1);

                    if (DateTime.UtcNow > WarStartingSoonUTC)
                    {
                        Flags.WarStartingSoon = true;
                    }

                    //      if (_cocApi != null)
                    //{
                    // _changed = true;
                    //}
                }
            }
        }

        public int TeamSize { get; set; }

        private WarClanAPIModel? _clan;

        public WarClanAPIModel? Clan
        {
            get { return _clan; }
            set
            {
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
            set
            {
                _opponent = value;
                if (Opponent != null)
                {
                    Clans.Add(Opponent);
                }
            }
        }

        private State _state;

        public State State
        {
            get
            {
                return _state;
            }

            set
            {
                if (_state != value)
                {
                    _state = value;

                    //if(_cocApi != null)
                    //{
                    //	_changed = true;
                    //}
                }
            }
        }







        [JsonIgnore]
        public IList<WarClanAPIModel> Clans { get; set; } = new List<WarClanAPIModel>();

        [JsonIgnore]
        public IDictionary<int, AttackAPIModel> Attacks { get; set; } = new Dictionary<int, AttackAPIModel>();

        /// <summary>
        /// This amalgamation is a composite key of the preparation start time and clan tags.
        /// </summary>
        [JsonIgnore]
        public string WarID { get; set; } = string.Empty;

        [JsonIgnore]
        public WarType WarType { get; set; } = WarType.Random;

        [JsonIgnore]
        public DateTime WarEndingSoonUTC { get; internal set; }

        [JsonIgnore]
        public DateTime WarStartingSoonUTC { get; internal set; }

        //[JsonIgnore]
        //public bool WarEndingSoon { get; internal set; } = false;

        //[JsonIgnore]
        //public bool WarStartingSoon { get; internal set; } = false;

        [JsonIgnore]
        public CurrentWarFlags Flags { get; internal set; } = new CurrentWarFlags();





        public DateTime DateTimeUTC { get; internal set; } = DateTime.UtcNow;

        public DateTime Expires { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;

        //public void SetExpiration()
        //{
        //    DateTimeUTC = DateTime.UtcNow;

        //    Expires = DateTime.UtcNow.AddSeconds(15);
        //}

        public void Initialize()
        {
            //_cocApi = cocApi;

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
                    if (Clans[0].DestructionPercentage == Clans[1].DestructionPercentage)
                    {
                        Clans[0].Result = Result.Draw;
                        Clans[1].Result = Result.Draw;
                    }
                    else if (Clans[0].DestructionPercentage > Clans[1].DestructionPercentage)
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
                else if (Clans[0].Stars > Clans[1].Stars)
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
                            Attacks.TryAdd(attack.Order, attack);

                            MemberAPIModel defendingBase = Clans.First(x => x.Tag != clan.Tag).Members.First(x => x.Tag == attack.DefenderTag);

                            defendingBase.Defenses.Add(attack);

                            clan.Attacks.Add(attack);

                            Clans.First(x => x.Tag != clan.Tag).Defenses.Add(attack);
                        }
                    }
                }
            }

            //Attacks = Attacks.OrderBy(x => x.Order).ToList();

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
            if (State == State.NotInWar) return true;

            if (State == State.WarEnded) return true;

            if (Clans[0].Members.All(m => m.Attacks?.Count() == 2) && Clans[1].Members.All(m => m.Attacks?.Count() == 2)) return true;

            return false;
        }

        public bool IsExpired()
        {
            if (DateTime.UtcNow > Expires)
            {
                return true;
            }
            return false;
        }




        private readonly object _updateLock = new object();

        internal void Update(CocApi cocApi, ClanAPIModel storedClan, ICurrentWarAPIModel? downloadedWar)
        {
            lock (_updateLock)
            {
                SendWarNotifications(cocApi, downloadedWar);

                UpdateWar(cocApi, downloadedWar);

                UpdateAttacks(cocApi, downloadedWar);
            }
        }

        private void SendWarNotifications(CocApi cocApi, ICurrentWarAPIModel? downloadedWar)
        {
            if (downloadedWar == null && Flags.WarIsAccessible)
            {
                Flags.WarIsAccessible = false;

                cocApi.WarIsAccessibleChangedEvent(this);
            }
            else if (downloadedWar != null && Flags.WarIsAccessible == false)
            {
                cocApi.WarIsAccessibleChangedEvent(this);
            }

            if (!Flags.WarStartingSoon && DateTime.UtcNow > WarStartingSoonUTC)
            {
                cocApi.WarStartingSoonEvent(this);

                Flags.WarEndingSoon = true;
            }

            if (!Flags.WarEndingSoon && DateTime.UtcNow > WarEndingSoonUTC)
            {
                cocApi.WarEndingSoonEvent(this);

                Flags.WarEndingSoon = true;
            }

            if (downloadedWar == null && EndTimeUTC < DateTime.UtcNow && Flags.WarEndNotSeen)
            {
                Flags.WarEndNotSeen = true;

                cocApi.WarEndNotSeenEvent(this);
            }
        }

        private void UpdateWar(CocApi cocApi, ICurrentWarAPIModel? downloadedWar)
        {
            if(downloadedWar == null) return;
            
            if (EndTimeUTC != downloadedWar.EndTimeUTC ||
                StartTimeUTC != downloadedWar.StartTimeUTC ||
                State != downloadedWar.State
            )
            {
                cocApi.WarChangedEvent(this, downloadedWar);

                EndTimeUTC = downloadedWar.EndTimeUTC;
                StartTimeUTC = downloadedWar.StartTimeUTC;
                State = downloadedWar.State;
            }
        }

        private void UpdateAttacks(CocApi cocApi, ICurrentWarAPIModel? downloadedWar)
        {
            if (downloadedWar == null) return;

            List<AttackAPIModel> newAttacks = new List<AttackAPIModel>();

            foreach (AttackAPIModel attack in downloadedWar.Attacks.Values)
            {
                if (Attacks.TryAdd(attack.Order, attack))
                {
                    newAttacks.Add(attack); //todo there are more lists that contain attacks
                }
            }

            if (newAttacks.Count() > 0)
            {
                cocApi.NewAttacksEvent(this, newAttacks);
            }
        }
    }
}
