using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

using devhl.CocApi.Converters;
using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models
{
    public class CurrentWarApiModel : Downloadable, IInitialize, ICurrentWarApiModel
    {
        private DateTime _endTimeUtc;

        [JsonPropertyName("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUtc
        {
            get
            {
                return _endTimeUtc;
            }

            set
            {
                if (_endTimeUtc != value)
                {
                    _endTimeUtc = value;

                    WarEndingSoonUtc = _endTimeUtc.AddHours(-1);

                    if (DateTime.UtcNow > WarEndingSoonUtc)
                    {
                        Flags.WarEndingSoon = true;
                    }
                }
            }
        }

        [JsonPropertyName("preparationStartTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime PreparationStartTimeUtc { get; set; }


        private DateTime _startTimeUtc;

        [JsonPropertyName("startTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime StartTimeUtc
        {
            get
            {
                return _startTimeUtc;
            }

            set
            {
                if (_startTimeUtc != value)
                {
                    _startTimeUtc = value;

                    WarStartingSoonUtc = _startTimeUtc.AddHours(-1);

                    if (DateTime.UtcNow > WarStartingSoonUtc)
                    {
                        Flags.WarStartingSoon = true;
                    }
                }
            }
        }

        public int TeamSize { get; set; }

        private WarClanApiModel? _clan;

        /// <summary>
        /// Do not use this property.  Instead, use the Clans property.
        /// </summary>
        [NotMapped]
        public WarClanApiModel? Clan
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

        private WarClanApiModel? _opponent;

        /// <summary>
        /// Do not use this property.  Instead, use the Clans property.
        /// </summary>
        [NotMapped]
        public WarClanApiModel? Opponent
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
        [ForeignKey(nameof(WarId))]
        
        public virtual IList<WarClanApiModel> Clans { get; set; } = new List<WarClanApiModel>();

        [JsonIgnore]
        [ForeignKey(nameof(WarId))]
        public virtual List<AttackApiModel> Attacks { get; set; } = new List<AttackApiModel>();

        [JsonIgnore]
        [Key]
        public string WarId { get; set; } = string.Empty;

        [JsonIgnore]
        public WarType WarType { get; set; } = WarType.Random;

        [JsonIgnore]
        public DateTime WarEndingSoonUtc { get; set; }

        [JsonIgnore]
        public DateTime WarStartingSoonUtc { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(WarId))]
        public virtual CurrentWarFlagsModel Flags { get; set; } = new CurrentWarFlagsModel();



        //public DateTime UpdatedAtUtc { get; set; }

        //public DateTime ExpiresAtUtc { get; set; }

        //public string EncodedUrl { get; set; } = string.Empty;

        //public DateTime? CacheExpiresAtUtc { get; set; }



        public void Initialize()
        {
            Clans = Clans.OrderBy(x => x.ClanTag).ToList();

            WarId = $"{PreparationStartTimeUtc};{Clans[0].ClanTag}";

            Flags.WarId = WarId;

            TimeSpan timeSpan = StartTimeUtc - PreparationStartTimeUtc;

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

            foreach (WarClanApiModel clan in Clans)
            {
                clan.WarId = WarId;

                foreach (WarVillageApiModel warVillage in clan.Villages.EmptyIfNull())
                {
                    warVillage.WarClanId = clan.WarClanId;

                    warVillage.ClanTag = clan.ClanTag;

                    warVillage.WarId = WarId;

                    foreach (AttackApiModel attack in warVillage.Attacks.EmptyIfNull())
                    {
                        attack.AttackerClanTag = clan.ClanTag;

                        attack.DefenderClanTag = Clans.First(c => c.ClanTag != clan.ClanTag).ClanTag;

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
                attack.WarId = WarId;

                attack.PreparationStartTimeUtc = PreparationStartTimeUtc;

                var attacksThisBase = Attacks.Where(a => a.AttackerClanTag == attack.AttackerClanTag && a.DefenderTag == attack.DefenderTag && a.AttackerTag != attack.AttackerTag).ToList();

                if (attacksThisBase.Count() == 0)
                {
                    attack.StarsGained = attack.Stars;
                }
                else
                {
                    attack.StarsGained = Math.Max(attack.Stars - attacksThisBase.OrderBy(a => a.Stars).FirstOrDefault().Stars, 0);

                    
                }

                foreach (var clan in Clans)
                {
                    WarVillageApiModel? attacker = clan.Villages.FirstOrDefault(m => m.VillageTag == attack.AttackerTag);

                    if (attacker != null) attack.AttackerClanTag = clan.ClanTag;

                    WarVillageApiModel? defender = clan.Villages.FirstOrDefault(m => m.VillageTag == attack.DefenderTag);

                    if (defender != null) attack.DefenderClanTag = clan.ClanTag;
                }
            }

            foreach (WarClanApiModel clan in Clans)
            {
                clan.DefenseCount = Attacks.Count(a => a.DefenderClanTag == clan.ClanTag);
            }

            if (PreparationStartTimeUtc != DateTime.MinValue)
            {
                Flags.WarIsAccessible = true;
            }

            if (State > WarState.Preparation)
            {
                Flags.WarAnnounced = true;

                Flags.WarStarted = true;

                Flags.WarStartingSoon = true;
            }

            if (State > WarState.InWar)
            {
                Flags.AttacksMissed = true;

                Flags.AttacksNotSeen = true;

                Flags.WarEnded = true;

                Flags.WarEndingSoon = true;

                Flags.WarEndNotSeen = true;

                Flags.WarEndSeen = true;
            }

            if (State == WarState.Preparation && WarStartingSoonUtc < DateTime.UtcNow)
            {
                Flags.WarStartingSoon = true;
            }

            if (State == WarState.InWar && WarEndingSoonUtc < DateTime.UtcNow)
            {
                Flags.WarEndingSoon = true;
            }
        }

        public bool WarIsOverOrAllAttacksUsed()
        {
            //if (State == WarState.NotInWar) return true;

            if (State == WarState.WarEnded) return true;

            if (Clans[0].Villages.All(m => m.Attacks?.Count() == 2) && Clans[1].Villages.All(m => m.Attacks?.Count() == 2)) return true;

            return false;
        }

        //public bool IsExpired()
        //{
        //    if (DateTime.UtcNow > ExpiresAtUtc)
        //    {
        //        return true;
        //    }

        //    return false;
        //}




        private readonly object _updateLock = new object();

        /// <summary>
        /// This is used internally to prevent a race condition which would announce a war twice.
        /// </summary>
        public readonly object NewWarLock = new object();

        internal void Update(CocApi cocApi, IWar? downloadedWar, ILeagueGroup? leagueGroupApiModel)
        {
            lock (_updateLock)
            {
                SendWarNotifications(cocApi, downloadedWar);

                if (ReferenceEquals(this, downloadedWar))
                {
                    return;
                }

                UpdateWar(cocApi, downloadedWar);

                UpdateAttacks(cocApi, downloadedWar);

                UpdateLeagueTeamSize(cocApi, leagueGroupApiModel);
            }
        }

        private void SendWarNotifications(CocApi cocApi, IWar? downloadedWar)
        {
            ICurrentWarApiModel? currentWar = downloadedWar as ICurrentWarApiModel;

            if (Flags.WarIsAccessible && (currentWar == null || currentWar.WarId != WarId))
            {
                Flags.WarIsAccessible = false;

                cocApi.WarIsAccessibleChangedEvent(this);
            }
            else if (!Flags.WarIsAccessible && (currentWar?.WarId == WarId))
            {
                Flags.WarIsAccessible = true;

                cocApi.WarIsAccessibleChangedEvent(this);
            }

            if (!Flags.WarStartingSoon && State == WarState.Preparation && DateTime.UtcNow > WarStartingSoonUtc)
            {
                Flags.WarStartingSoon = true;

                cocApi.WarStartingSoonEvent(this);
            }

            if (!Flags.WarEndingSoon && State == WarState.InWar && DateTime.UtcNow > WarEndingSoonUtc)
            {
                Flags.WarEndingSoon = true;

                cocApi.WarEndingSoonEvent(this);
            }

            if (!Flags.WarEndNotSeen && (currentWar == null || WarId != currentWar.WarId) && EndTimeUtc < DateTime.UtcNow)
            {
                Flags.WarEndNotSeen = true;

                cocApi.WarEndNotSeenEvent(this);
            }

            if (!Flags.WarStarted && StartTimeUtc < DateTime.UtcNow)
            {
                Flags.WarStarted = true;

                cocApi.WarStartedEvent(this);
            }

            if (!Flags.WarEnded && EndTimeUtc < DateTime.UtcNow)
            {
                Flags.WarEnded = true;

                cocApi.WarEndedEvent(this);
            }

            if (!Flags.WarEndSeen && State == WarState.InWar && currentWar?.State == WarState.WarEnded)
            {
                Flags.WarEndSeen = true;

                cocApi.WarEndSeenEvent(this);
            }
        }

        private void UpdateWar(CocApi cocApi, IWar? downloadedWar)
        {
            ICurrentWarApiModel? currentWar = downloadedWar as ICurrentWarApiModel;

            if (currentWar == null || currentWar.WarId != WarId) return;
            
            //foreach(var clan in Clans)
            //{
            //    clan.BadgeUrls = downloadedWar.Clans.First(c => c.ClanTag == clan.ClanTag).BadgeUrls;

            //    clan.ClanLevel = downloadedWar.Clans.First(c => c.ClanTag == clan.ClanTag).ClanLevel;

            //    clan.Name = downloadedWar.Clans.First(c => c.ClanTag == clan.ClanTag).Name;
            //}

            if (EndTimeUtc != currentWar.EndTimeUtc ||
                StartTimeUtc != currentWar.StartTimeUtc ||
                State != currentWar.State
            )
            {
                cocApi.WarChangedEvent(this, currentWar);

                //EndTimeUtc = downloadedWar.EndTimeUtc;
                //StartTimeUtc = downloadedWar.StartTimeUtc;
                //State = downloadedWar.State;
            }
        }

        private void UpdateAttacks(CocApi cocApi, IWar? downloadedWar)
        {
            ICurrentWarApiModel? currentWar = downloadedWar as ICurrentWarApiModel;

            if (currentWar == null || currentWar.WarId != WarId) return;

            List<AttackApiModel> newAttacks = currentWar.Attacks.Where(a => a.Order > Attacks.Count()).ToList();

            

            //foreach (AttackApiModel attack in downloadedWar.Attacks)
            //{                
            //    if (!Attacks.Any(a => a.Order == attack.Order))
            //    {
            //        Attacks.Add(attack);
            //    }
            //}

            //foreach (WarClanApiModel clan in Clans)
            //{
            //    foreach (WarVillageApiModel warVillage in clan.Villages.EmptyIfNull())
            //    {
            //        foreach (AttackApiModel downloadedAttack in downloadedWar.Attacks.Where(a => a.AttackerTag == warVillage.VillageTag))
            //        {
            //            if (!warVillage.Attacks.Any(a => a.Order == downloadedAttack.Order))
            //            {
            //                if (warVillage.Attacks == null)
            //                {
            //                    warVillage.Attacks = new List<AttackApiModel>();
            //                }

            //                warVillage.Attacks.Add(downloadedAttack);
            //            }
            //        }
            //    }
            //}

            //foreach (var clan in Clans)
            //{
            //    clan.AttackCount = downloadedWar.Clans.First(c => c.ClanTag == clan.ClanTag).AttackCount;

            //    clan.DefenseCount = downloadedWar.Clans.First(c => c.ClanTag == clan.ClanTag).DefenseCount;

            //    clan.DestructionPercentage = downloadedWar.Clans.First(c => c.ClanTag == clan.ClanTag).DestructionPercentage;

            //    clan.Stars = downloadedWar.Clans.First(c => c.ClanTag == clan.ClanTag).Stars;
            //}

            cocApi.NewAttacksEvent(currentWar, newAttacks);
        }

        private void UpdateLeagueTeamSize(CocApi cocApi, ILeagueGroup? leagueGroup)
        {
            if (leagueGroup is LeagueGroupApiModel leagueGroupApiModel)
            {
                if (leagueGroupApiModel.TeamSize > 15) return;

                if (Clans.Any(c => c.AttackCount > 15))
                {
                    leagueGroupApiModel.TeamSize = 30;

                    cocApi.LeagueGroupTeamSizeChangeDetectedEvent(leagueGroupApiModel);
                }
            }            
        }
    }
}
