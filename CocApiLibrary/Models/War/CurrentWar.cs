using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Newtonsoft.Json;

using devhl.CocApi.Converters;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi.Models.War
{
    public class CurrentWar : Downloadable, IInitialize, IWar, ICurrentWar
    {
        [JsonProperty("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUtc { get; private set; }


        [JsonProperty("preparationStartTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime PreparationStartTimeUtc { get; private set; }


        [JsonProperty("startTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime StartTimeUtc { get; private set; }


        [JsonProperty]
        public int TeamSize { get; internal set; }


        [JsonProperty]
        internal WarClan? Clan { get; private set; }

        [JsonProperty]
        internal WarClan? Opponent { get; private set; }


        [JsonProperty]
        public WarState State { get; private set; }


        [JsonProperty]
        public IList<WarClan> WarClans { get; internal set; } = new List<WarClan>();

        [JsonProperty]
        public List<Attack> Attacks { get; internal set; } = new List<Attack>();

        /// <summary>
        /// This value is used internally to identify unique wars.
        /// </summary>
        [JsonProperty]
        public string WarKey { get; private set; } = string.Empty;

        [JsonProperty]
        public WarType WarType { get; internal set; } = WarType.Random;

        [JsonProperty]
        public DateTime WarEndingSoonUtc { get; internal set; }

        [JsonProperty]
        public DateTime WarStartingSoonUtc { get; internal set; }

        [JsonProperty]
        public CurrentWarFlags Flags { get; internal set; } = new CurrentWarFlags();

        public void Initialize(CocApi cocApi)
        {
            SetWarEndingSoonWarning();

            SetWarStartingSoonWarning();

            if (Clan != null) WarClans.Add(Clan);

            if (Opponent != null) WarClans.Add(Opponent);

            WarClans = WarClans.OrderBy(x => x.ClanTag).ToList();

            WarKey = $"{PreparationStartTimeUtc};{WarClans[0].ClanTag}";

            Flags.WarKey = WarKey;

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
                if (WarClans[0].Stars == WarClans[1].Stars)
                {
                    if (WarClans[0].DestructionPercentage == WarClans[1].DestructionPercentage)
                    {
                        WarClans[0].Result = Result.Draw;
                        WarClans[1].Result = Result.Draw;
                    }
                    else if (WarClans[0].DestructionPercentage > WarClans[1].DestructionPercentage)
                    {
                        WarClans[0].Result = Result.Win;
                        WarClans[1].Result = Result.Lose;
                    }
                    else
                    {
                        WarClans[0].Result = Result.Lose;
                        WarClans[1].Result = Result.Win;
                    }
                }
                else if (WarClans[0].Stars > WarClans[1].Stars)
                {
                    WarClans[0].Result = Result.Win;
                    WarClans[1].Result = Result.Lose;
                }
                else
                {
                    WarClans[0].Result = Result.Lose;
                    WarClans[1].Result = Result.Win;
                }
            }

            foreach (WarClan clan in WarClans)
            {
                clan.WarKey = WarKey;

                clan.Initialize(cocApi);

                clan.WarVillages = clan.WarVillages.OrderBy(wv => wv.RosterPosition);

                int i = 1;

                foreach (WarVillage warVillage in clan.WarVillages.EmptyIfNull())
                {
                    warVillage.MapPosition = i;

                    i++;

                    foreach (Attack attack in warVillage.Attacks.EmptyIfNull())
                    {
                        attack.AttackerClanTag = clan.ClanTag;

                        attack.DefenderClanTag = WarClans.First(c => c.ClanTag != clan.ClanTag).ClanTag;

                        if (!Attacks.Any(a => a.Order == attack.Order))
                        {
                            Attacks.Add(attack);
                        }
                    }
                }
            }

            Attacks = Attacks.OrderBy(a => a.Order).ToList();

            var attacksByDefenderTag = Attacks.GroupBy(a => a.DefenderTag);

            foreach (var defendingVillage in attacksByDefenderTag)
            {
                defendingVillage.OrderBy(d => d.Order).First().Fresh = true;
            }

            foreach (var attack in Attacks)
            {
                attack.WarKey = WarKey;

                attack.PreparationStartTimeUtc = PreparationStartTimeUtc;

                var attacksThisBase = Attacks.Where(a => a.AttackerClanTag == attack.AttackerClanTag && 
                                                         a.DefenderTag == attack.DefenderTag && 
                                                         a.Order < attack.Order).ToList();

                if (attacksThisBase.Count == 0)
                {
                    attack.StarsGained = attack.Stars;
                }
                else
                {
                    attack.StarsGained = Math.Max(attack.Stars!.Value - attacksThisBase.OrderByDescending(a => a.Stars).First().Stars!.Value, 0);
                }

                foreach (var clan in WarClans)
                {
                    WarVillage? attacker = clan.WarVillages.FirstOrDefault(m => m.VillageTag == attack.AttackerTag);

                    if (attacker != null)
                    {
                        attack.AttackerClanTag = clan.ClanTag;

                        attack.AttackerMapPosition = attacker.RosterPosition;

                        attack.AttackerTownHallLevel = attacker.TownHallLevel;
                    }

                    WarVillage? defender = clan.WarVillages.FirstOrDefault(m => m.VillageTag == attack.DefenderTag);

                    if (defender != null)
                    {
                        attack.DefenderClanTag = clan.ClanTag;

                        attack.DefenderMapPosition = defender.RosterPosition;

                        attack.DefenderTownHallLevel = defender.TownHallLevel;
                    }
                }
            }

            foreach (WarClan clan in WarClans)
            {
                clan.DefenseCount = Attacks.Count(a => a.DefenderClanTag == clan.ClanTag);
            }

            if (PreparationStartTimeUtc != DateTime.MinValue)
            {
                Flags.WarIsAccessible = true;
            }

            if (State > WarState.Preparation)
            {
                //Flags.WarAnnounced = true;

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

        private void SetWarEndingSoonWarning()
        {
            WarEndingSoonUtc = EndTimeUtc.AddHours(-1);

            if (DateTime.UtcNow > WarEndingSoonUtc)
            {
                Flags.WarEndingSoon = true;
            }
        }

        private void SetWarStartingSoonWarning()
        {
            WarStartingSoonUtc = StartTimeUtc.AddHours(-1);

            if (DateTime.UtcNow > WarStartingSoonUtc)
            {
                Flags.WarStartingSoon = true;
            }
        }

        public bool WarIsOverOrAllAttacksUsed()
        {
            if (State == WarState.WarEnded) return true;

            if (this is LeagueWar leagueWar)
            {
                if (WarClans[0].WarVillages.All(m => m.Attacks?.Count == 1) && WarClans[1].WarVillages.All(m => m.Attacks?.Count == 1)) return true;
            }
            else
            {
                if (WarClans[0].WarVillages.All(m => m.Attacks?.Count == 2) && WarClans[1].WarVillages.All(m => m.Attacks?.Count == 2)) return true;
            }

            return false;
        }

        private readonly object _lockObject = new object();

        internal readonly object _announceWarLock = new object();

        internal void Update(CocApi cocApi, IWar? downloadedWar)
        {
            lock (_lockObject)
            {
                PreUpdateAnnouncements(cocApi);

                if (ReferenceEquals(this, downloadedWar)) return;

                if (downloadedWar is CurrentWar currentWar)
                {
                    //the type of war should only be decided on the wars fist download
                    //to prevent maintenance breaks from changing the type to random
                    currentWar.WarType = this.WarType;
                }

                UpdateWar(cocApi, downloadedWar);

                UpdateAttacks(cocApi, downloadedWar);

                PostUpdateAnnouncements(cocApi, downloadedWar);
            }
        }

        private void PreUpdateAnnouncements(CocApi cocApi)
        {
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
        }

        private void PostUpdateAnnouncements(CocApi cocApi, IWar? downloadedWar)
        {
            CurrentWar? currentWar = downloadedWar as CurrentWar;

            if (Flags.WarIsAccessible && (downloadedWar is PrivateWarLog || currentWar?.WarKey != WarKey))
            {
                Flags.WarIsAccessible = false;

                cocApi.WarIsAccessibleChangedEvent(this, false);
            }
            else if (!Flags.WarIsAccessible && currentWar != null && currentWar.WarKey == WarKey)
            {
                Flags.WarIsAccessible = true;

                cocApi.WarIsAccessibleChangedEvent(this, true);
            }

            if (!Flags.WarEndNotSeen && (currentWar == null || WarKey != currentWar.WarKey) && EndTimeUtc < DateTime.UtcNow)
            {
                Flags.WarEndNotSeen = true;

                cocApi.WarEndNotSeenEvent(this);
            }

            if (!Flags.WarEndSeen && State == WarState.InWar && currentWar?.State == WarState.WarEnded)
            {
                Flags.WarEndSeen = true;

                cocApi.WarEndSeenEvent(this);
            }
        }

        private void CreateMissedAttacksCurrentWar(CurrentWar downloadedWar, WarClan downloadedWarClan)
        {
            foreach (var downloadedWarVillage in downloadedWarClan.WarVillages.EmptyIfNull())
            {
                downloadedWarVillage.Attacks ??= new List<Attack>();

                if (downloadedWarVillage.Attacks.Count() == 0) CreateMissedAttack(downloadedWar, downloadedWarVillage);

                if (downloadedWarVillage.Attacks.Count() == 1) CreateMissedAttack(downloadedWar, downloadedWarVillage);
            }

            return;
        }

        private void CreateMissedAttacksLeagueWar(CurrentWar downloadedWar, WarClan downloadedWarClan)
        {
            foreach (var attacker in downloadedWarClan.WarVillages.EmptyIfNull())
            {
                attacker.Attacks ??= new List<Attack>();

                if (attacker.Attacks.Count == 0) CreateMissedAttack(downloadedWar, attacker);
            }
        }



        private Attack CreateMissedAttack(CurrentWar downloadedWar, WarVillage downloadedWarVillage)
        {
            Attack attack = new Attack
            {
                AttackerClanTag = downloadedWarVillage.ClanTag,

                AttackerMapPosition = downloadedWarVillage.RosterPosition,

                AttackerTag = downloadedWarVillage.VillageTag,

                AttackerTownHallLevel = downloadedWarVillage.TownHallLevel,

                Missed = true,

                PreparationStartTimeUtc = PreparationStartTimeUtc,

                WarKey = WarKey,

                DefenderClanTag = WarClans.First(c => c.ClanTag != downloadedWarVillage.ClanTag).ClanTag
            };

            downloadedWar.Attacks.Add(attack);

            downloadedWarVillage.Attacks?.Add(attack);

            return attack;
        }

        private void UpdateWar(CocApi cocApi, IWar? downloadedWar)
        {
            if (!(downloadedWar is CurrentWar currentWar) || currentWar.WarKey != WarKey) return;

            if (EndTimeUtc != currentWar.EndTimeUtc ||
                StartTimeUtc != currentWar.StartTimeUtc ||
                State != currentWar.State
            )
            {
                cocApi.WarChangedEvent(this, currentWar);
            }
        }

        private void UpdateAttacks(CocApi cocApi, IWar? downloadedWar)
        {
            if (!(downloadedWar is CurrentWar currentWar) || currentWar.WarKey != WarKey) return; 

            List<Attack> newAttacks = currentWar.Attacks.Where(a => a.Order > Attacks.Count).ToList();

            if (currentWar.State == WarState.WarEnded)
            {
                foreach (var downloadedWarClan in currentWar.WarClans)
                {
                    if (this is LeagueWar)
                    {
                        CreateMissedAttacksLeagueWar(currentWar, downloadedWarClan);
                    }
                    else
                    {
                        CreateMissedAttacksCurrentWar(currentWar, downloadedWarClan);
                    }
                }
            }

            newAttacks.AddRange(currentWar.Attacks.Where(a => a.Missed == true));

            cocApi.NewAttacksEvent(currentWar, newAttacks);
        }

        public override string ToString() => PreparationStartTimeUtc.ToString();
    }
}
