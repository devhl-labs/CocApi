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

        private WarState _state;

        public WarState State
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
                }
            }
        }







        [JsonIgnore]
        public IList<WarClanAPIModel> Clans { get; set; } = new List<WarClanAPIModel>();

        [JsonIgnore]
        public List<AttackAPIModel> Attacks { get; set; } = new List<AttackAPIModel>();

        /// <summary>
        /// This amalgamation is a composite key of the preparation start time and clan tags in alphabetical order.
        /// </summary>
        [JsonIgnore]
        public string WarID { get; set; } = string.Empty;

        [JsonIgnore]
        public WarType WarType { get; set; } = WarType.Random;

        [JsonIgnore]
        public DateTime WarEndingSoonUTC { get; internal set; }

        [JsonIgnore]
        public DateTime WarStartingSoonUTC { get; internal set; }

        [JsonIgnore]
        public CurrentWarFlags Flags { get; internal set; } = new CurrentWarFlags();





        public DateTime DateTimeUTC { get; internal set; } = DateTime.UtcNow;

        public DateTime Expires { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;



        public void Initialize()
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
                foreach (MemberAPIModel member in clan.Members.EmptyIfNull())
                {
                    foreach (AttackAPIModel attack in member.Attacks.EmptyIfNull())
                    {
                        if (!Attacks.Any(a => a.Order == attack.Order))
                        {
                            Attacks.Add(attack);
                        }
                    }
                }
            }

            Attacks = Attacks.OrderBy(a => a.Order).ToList();

            var attacksByDefenderTag = Attacks.GroupBy(a => a.DefenderTag);
            
            foreach(var defendingVillage in attacksByDefenderTag)
            {
                defendingVillage.OrderBy(d => d.Order).First().Fresh = true;
            }

            foreach(var attack in Attacks)
            {
                foreach(var clan in Clans)
                {
                    MemberAPIModel? attacker = clan.Members.FirstOrDefault(m => m.Tag == attack.AttackerTag);

                    if (attacker != null) attack.AttackerClanTag = clan.Tag;

                    MemberAPIModel? defender = clan.Members.FirstOrDefault(m => m.Tag == attack.DefenderTag);

                    if (defender != null) attack.DefenderClanTag = clan.Tag;
                }
            }

            foreach (WarClanAPIModel clan in Clans)
            {
                clan.DefenseCount = Attacks.Count(a => a.DefenderClanTag == clan.Tag);
            }

        }

        public bool WarIsOverOrAllAttacksUsed()
        {
            if (State == WarState.NotInWar) return true;

            if (State == WarState.WarEnded) return true;

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

        private readonly object _newWarLock = new object();

        internal void Update(CocApi cocApi, ICurrentWarAPIModel? downloadedWar, LeagueGroupAPIModel? leagueGroupAPIModel)
        {
            lock (_updateLock)
            {
                SendWarNotifications(cocApi, downloadedWar);

                UpdateWar(cocApi, downloadedWar);

                UpdateAttacks(cocApi, downloadedWar);

                UpdateLeagueTeamSize(cocApi, leagueGroupAPIModel);

                if (downloadedWar?.WarID == WarID)
                {
                    Expires = downloadedWar.Expires;

                    DateTimeUTC = downloadedWar.DateTimeUTC;
                }
            }
        }



        internal void AnnounceNewWar(CocApi cocApi)
        {
            lock (_newWarLock)
            {
                if (Flags.WarAnnounced) return;
                
                foreach(var clan in Clans)
                {
                    if (cocApi.AllClans.TryGetValue(clan.Tag, out ClanAPIModel storedClan))
                    {
                        //we only announce wars if this flag is false to avoid spamming new war events when the program starts.
                        if (storedClan.AnnounceWars)
                        {
                            cocApi.NewWarEvent(this);

                            break;
                        }
                    }
                }                

                Flags.WarAnnounced = true;                
            }
        }

        private void SendWarNotifications(CocApi cocApi, ICurrentWarAPIModel? downloadedWar)
        {
            if (Flags.WarIsAccessible && (downloadedWar == null || downloadedWar.WarID != WarID))
            {
                Flags.WarIsAccessible = false;

                cocApi.WarIsAccessibleChangedEvent(this);
            }
            else if (!Flags.WarIsAccessible && (downloadedWar?.WarID == WarID))
            {
                Flags.WarIsAccessible = true;

                cocApi.WarIsAccessibleChangedEvent(this);
            }

            if (!Flags.WarStartingSoon && State == WarState.Preparation && DateTime.UtcNow > WarStartingSoonUTC)
            {
                cocApi.WarStartingSoonEvent(this);

                Flags.WarEndingSoon = true;
            }

            if (!Flags.WarEndingSoon && State == WarState.InWar && DateTime.UtcNow > WarEndingSoonUTC)
            {
                cocApi.WarEndingSoonEvent(this);

                Flags.WarEndingSoon = true;
            }

            if (!Flags.WarEndNotSeen && (downloadedWar == null || WarID != downloadedWar.WarID) && EndTimeUTC < DateTime.UtcNow)
            {
                Flags.WarEndNotSeen = true;

                cocApi.WarEndNotSeenEvent(this);
            }

            if (!Flags.WarStarted && StartTimeUTC < DateTime.UtcNow)
            {
                Flags.WarStarted = true;

                cocApi.WarStartedEvent(this);
            }

            if (!Flags.WarEnded && EndTimeUTC < DateTime.UtcNow)
            {
                Flags.WarEnded = true;

                cocApi.WarEndedEvent(this);
            }

            if (!Flags.WarEndSeen && State == WarState.InWar && downloadedWar?.State == WarState.WarEnded)
            {
                Flags.WarEndSeen = true;

                cocApi.WarEndSeenEvent(this);
            }
        }

        private void UpdateWar(CocApi cocApi, ICurrentWarAPIModel? downloadedWar)
        {
            if (downloadedWar == null || downloadedWar.WarID != WarID) return;
            
            foreach(var clan in Clans)
            {
                clan.BadgeUrls = downloadedWar.Clans.First(c => c.Tag == clan.Tag).BadgeUrls;

                clan.ClanLevel = downloadedWar.Clans.First(c => c.Tag == clan.Tag).ClanLevel;

                clan.Name = downloadedWar.Clans.First(c => c.Tag == clan.Tag).Name;
            }

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
            if (downloadedWar == null || downloadedWar.WarID != WarID) return;

            List<AttackAPIModel> newAttacks = new List<AttackAPIModel>();

            foreach (AttackAPIModel attack in downloadedWar.Attacks)
            {                
                if (!Attacks.Any(a => a.Order == attack.Order))
                {
                    Attacks.Add(attack);
                }
            }

            foreach(WarClanAPIModel clan in Clans)
            {
                foreach(MemberAPIModel member in clan.Members.EmptyIfNull())
                {
                    foreach(AttackAPIModel downloadedAttack in downloadedWar.Attacks.Where(a => a.AttackerTag == member.Tag))
                    {
                        if (!member.Attacks.Any(a => a.Order == downloadedAttack.Order))
                        {
                            if (member.Attacks == null)
                            {
                                member.Attacks = new List<AttackAPIModel>();
                            }

                            member.Attacks.Add(downloadedAttack);
                        }
                    }
                }
            }

            foreach (var clan in Clans)
            {
                clan.AttackCount = downloadedWar.Clans.First(c => c.Tag == clan.Tag).AttackCount;

                clan.DefenseCount = downloadedWar.Clans.First(c => c.Tag == clan.Tag).DefenseCount;

                clan.DestructionPercentage = downloadedWar.Clans.First(c => c.Tag == clan.Tag).DestructionPercentage;

                clan.Stars = downloadedWar.Clans.First(c => c.Tag == clan.Tag).Stars;
            }

            cocApi.NewAttacksEvent(this, newAttacks);
        }

        private void UpdateLeagueTeamSize(CocApi cocApi, LeagueGroupAPIModel? leagueGroupAPIModel)
        {
            if (leagueGroupAPIModel == null) return;

            if (leagueGroupAPIModel.TeamSize > 15) return;

            if (Clans.Any(c => c.AttackCount > 15))
            {
                leagueGroupAPIModel.TeamSize = 30;

                cocApi.LeagueGroupTeamSizeChangeDetectedEvent(leagueGroupAPIModel);
            }
            
        }
    }
}
