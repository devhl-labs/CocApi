using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using CocApi.Cache.Converters;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using CocApi.Cache.Exceptions;

namespace CocApi.Cache.Models.Wars
{
    public class CurrentWar : Downloadable, IInitialize, IWar, ICurrentWar
    {
        public static string Url(string clanTag)
        {
            if (CocApiClient_old.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar";
        }

        [JsonProperty("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUtc { get; internal set; }


        [JsonProperty("preparationStartTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime PreparationStartTimeUtc { get; internal set; }


        [JsonProperty("startTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime StartTimeUtc { get; internal set; }


        [JsonProperty]
        public int TeamSize { get; internal set; }


        [JsonProperty]
        internal WarClan? Clan { get; private set; }

        [JsonProperty]
        internal WarClan? Opponent { get; private set; }


        [JsonProperty]
        public WarState State { get; internal set; }


        [JsonProperty]
        public IList<WarClan> WarClans { get; internal set; } = new List<WarClan>();

        [JsonProperty]
        public List<Attack> Attacks { get; internal set; } = new List<Attack>();

        ///// <summary>
        ///// This value is used internally to identify unique wars.
        ///// </summary>
        //[JsonProperty]
        //public string WarKey { get; internal set; } = string.Empty;

        public string WarKey() => $"{PreparationStartTimeUtc};{WarClans[0].ClanTag}";

        [JsonProperty]
        public WarType WarType { get; internal set; } = WarType.Random;

        [JsonProperty]
        public DateTime WarEndingSoonUtc { get; internal set; }

        [JsonProperty]
        public DateTime WarStartingSoonUtc { get; internal set; }

        public void Initialize(CocApiClient_old cocApi)
        {
            WarStartingSoonUtc = StartTimeUtc.AddHours(-1);
            WarEndingSoonUtc = EndTimeUtc.AddHours(-1);

            if (Clan != null) WarClans.Add(Clan);

            if (Opponent != null) WarClans.Add(Opponent);

            WarClans = WarClans.OrderBy(x => x.ClanTag).ToList();

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
                        WarClans[0].Result = Result.Tie;
                        WarClans[1].Result = Result.Tie;
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
                clan.WarKey = WarKey();

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
                clan.DefenseCount = Attacks.Count(a => a.DefenderClanTag == clan.ClanTag);            
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

        [JsonProperty]
        public Announcements Announcements { get; private set; }

        internal void Update(CocApiClient_old cocApi, IWar? fetchedWar, Announcements announcements)
        {
            Announcements = announcements;

            PreUpdateAnnouncements(cocApi, fetchedWar);

            if (ReferenceEquals(this, fetchedWar) || fetchedWar == null) 
                return;

            if (fetchedWar is CurrentWar fetchedCurrentWar && WarKey() == fetchedCurrentWar.WarKey())
                fetchedCurrentWar.WarType = WarType;

            UpdateWar(cocApi, fetchedWar);

            UpdateAttacks(cocApi, fetchedWar);

            WarEndSeenAnnouncement(cocApi, fetchedWar);
        }

        private void PreUpdateAnnouncements(CocApiClient_old cocApi, IWar? fetchedWar)
        {
            cocApi.OnLog(new CurrentWarLogEventArgs(nameof(CurrentWar), nameof(PreUpdateAnnouncements), this, fetchedWar));

            CurrentWar? currentWar = fetchedWar as CurrentWar;

            if (Announcements.HasFlag(Announcements.WarStartingSoon) == false && DateTime.UtcNow > WarStartingSoonUtc && DateTime.UtcNow < StartTimeUtc)
            {
                Announcements |= Announcements.WarStartingSoon;
                cocApi.Wars.OnWarStartingSoon(this);
            }

            if (Announcements.HasFlag(Announcements.WarEndingSoon) == false && DateTime.UtcNow > WarEndingSoonUtc && DateTime.UtcNow < EndTimeUtc)
            {
                Announcements |= Announcements.WarEndingSoon;
                cocApi.Wars.OnWarEndingSoon(this);
            }

            if (Announcements.HasFlag(Announcements.WarStarted) == false && DateTime.UtcNow > StartTimeUtc && DateTime.UtcNow < EndTimeUtc)
            {
                Announcements |= Announcements.WarStarted;
                cocApi.Wars.OnWarStarted(this);
            }

            if (Announcements.HasFlag(Announcements.WarEnded) == false && DateTime.UtcNow > EndTimeUtc && DateTime.UtcNow.AddHours(1).Day == EndTimeUtc.Day)
            {
                Announcements |= Announcements.WarEnded;

                cocApi.Wars.OnWarEnded(this);
            }

            if (Announcements.HasFlag(Announcements.WarIsAccessible) && (fetchedWar is PrivateWarLog ||  fetchedWar is NotInWar || currentWar?.WarKey() != WarKey()))
            {
                Announcements &= ~Announcements.WarIsAccessible;
                cocApi.Wars.OnWarIsAccessibleChanged(this, false);
            }
            else if (Announcements.HasFlag(Announcements.WarIsAccessible) == false && currentWar != null && currentWar.WarKey() == WarKey())
            {
                Announcements |= Announcements.WarIsAccessible;
                cocApi.Wars.OnWarIsAccessibleChanged(currentWar, true);
            }

            if (Announcements.HasFlag(Announcements.WarEndNotSeen) == false && 
                fetchedWar is NotInWar &&
                DateTime.UtcNow > EndTimeUtc &&
                WarIsOverOrAllAttacksUsed() == false)
            {
                Announcements |= Announcements.WarEndNotSeen;

                if (DateTime.UtcNow.Day == EndTimeUtc.Day)
                    cocApi.Wars.OnWarEndNotSeen(this);
            }
        }

        private void WarEndSeenAnnouncement(CocApiClient_old cocApi, IWar? fetchedWar)
        {
            cocApi.OnLog(new CurrentWarLogEventArgs(nameof(CurrentWar), nameof(WarEndSeenAnnouncement), this, fetchedWar));

            if (DateTime.UtcNow < EndTimeUtc && Announcements.HasFlag(Announcements.WarEndSeen))
                return;

            CurrentWar? fetchedCurrentWar = fetchedWar as CurrentWar;

            if ((Announcements.HasFlag(Announcements.WarEndSeen) == false &&
                fetchedCurrentWar != null && 
                fetchedCurrentWar.WarIsOverOrAllAttacksUsed()) == true ||
                (WarKey() == fetchedCurrentWar?.WarKey() &&
                State != WarState.WarEnded && 
                fetchedCurrentWar.State == WarState.WarEnded))
            {
                Announcements |= Announcements.WarEndSeen;

                cocApi.Wars.OnWarEndSeen(fetchedCurrentWar ?? this);
            }

            if (fetchedWar is WarLogEntry entry && Announcements.HasFlag(Announcements.AddedToWarLog) == false)
            {
                Announcements |= Announcements.AddedToWarLog;

                State = WarState.WarEnded;

                cocApi.Wars.OnAddedToWarLog(entry, this);
            }
        }

        private void CreateMissedAttacks(CocApiClient_old cocApi, CurrentWar fetchedCurrentWar, WarClan fetchedWarClan, int numberOfAttacksAllowed)
        {
            cocApi.OnLog(new CurrentWarLogEventArgs(nameof(CurrentWar), nameof(CreateMissedAttacks), this, fetchedCurrentWar));

            foreach (var downloadedWarVillage in fetchedWarClan.WarVillages.EmptyIfNull())
            {
                downloadedWarVillage.Attacks ??= new List<Attack>();

                while(downloadedWarVillage.Attacks.Count() < numberOfAttacksAllowed)
                {
                    CreateMissedAttack(cocApi, fetchedCurrentWar, downloadedWarVillage);
                }
            }

            return;
        }

        private Attack CreateMissedAttack(CocApiClient_old cocApi, CurrentWar fetchedCurrentWar, WarVillage fetchedWarVillage)
        {
            cocApi.OnLog(new CurrentWarLogEventArgs(nameof(CurrentWar), nameof(CreateMissedAttack), this, fetchedCurrentWar));

            Attack attack = new Attack
            {
                AttackerClanTag = fetchedWarVillage.ClanTag,

                AttackerMapPosition = fetchedWarVillage.RosterPosition,

                AttackerTag = fetchedWarVillage.VillageTag,

                AttackerTownHallLevel = fetchedWarVillage.TownHallLevel,

                //PreparationStartTimeUtc = PreparationStartTimeUtc,

                //WarKey = WarKey(),

                DefenderClanTag = WarClans.First(c => c.ClanTag != fetchedWarVillage.ClanTag).ClanTag
            };

            fetchedCurrentWar.Attacks.Add(attack);

            fetchedWarVillage.Attacks?.Add(attack);

            return attack;
        }

        private void UpdateWar(CocApiClient_old cocApi, IWar? fetchedWar)
        {
            cocApi.OnLog(new CurrentWarLogEventArgs(nameof(CurrentWar), nameof(UpdateWar), this, fetchedWar));

            if (fetchedWar is CurrentWar currentWar && WarKey() == currentWar.WarKey())
            {
                if (EndTimeUtc != currentWar.EndTimeUtc ||
                    StartTimeUtc != currentWar.StartTimeUtc ||
                    State != currentWar.State
                )
                {
                    cocApi.Wars.OnWarChanged(currentWar, this);
                }
            }
        }

        private void UpdateAttacks(CocApiClient_old cocApi, IWar? fetchedWar)
        {
            cocApi.OnLog(new CurrentWarLogEventArgs(nameof(CurrentWar), nameof(UpdateAttacks), this, fetchedWar));

            if (fetchedWar is CurrentWar downloadedCurrentWar && WarKey() == downloadedCurrentWar.WarKey())
            {
                List<Attack> newAttacks = downloadedCurrentWar.Attacks.Where(a => a.Order > Attacks.Count).ToList();

                if (downloadedCurrentWar.State == WarState.WarEnded)
                {
                    foreach (var downloadedWarClan in downloadedCurrentWar.WarClans)
                    {
                        if (this is LeagueWar)
                        {
                            CreateMissedAttacks(cocApi, downloadedCurrentWar, downloadedWarClan, 1);
                        }
                        else
                        {
                            CreateMissedAttacks(cocApi, downloadedCurrentWar, downloadedWarClan, 2);
                        }
                    }
                }

                newAttacks.AddRange(downloadedCurrentWar.Attacks.Where(a => a.DefenderTag == null));

                cocApi.Wars.OnNewAttacks(downloadedCurrentWar, newAttacks);
            }

            if (fetchedWar is WarLogEntry entry)
            {
                List<Attack> attacks = new List<Attack>();

                if (entry.Clan?.ClanTag != null && 
                    entry.Clan.AttackCount != null &&
                    entry.Clan.AttackCount == Attacks.Count(a => a.AttackerClanTag == entry.Clan.ClanTag))
                {
                    CreateMissedAttacks(cocApi, this, WarClans.First(wc => wc.ClanTag == entry.Clan.ClanTag), 2);
                }
                
                if (entry.Opponent?.ClanTag != null &&
                    entry.Opponent.AttackCount != null &&
                    entry.Opponent.AttackCount == Attacks.Count(a => a.AttackerClanTag == entry.Opponent.ClanTag))
                {
                    CreateMissedAttacks(cocApi, this, WarClans.First(wc => wc.ClanTag == entry.Opponent.ClanTag), 2);
                }

                cocApi.Wars.OnNewAttacks(this, Attacks.Where(a => a.DefenderTag == null).ToList());
            }
        }

        public override string ToString() => PreparationStartTimeUtc.ToString();
    }
}
