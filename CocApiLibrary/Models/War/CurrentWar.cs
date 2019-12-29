using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Newtonsoft.Json;

using devhl.CocApi.Converters;
using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.War
{
    public class CurrentWar : Downloadable, IInitialize, IActiveWar
    {
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("endTime")]
        public DateTime EndTimeUtc { get; }


        [JsonProperty("preparationStartTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime PreparationStartTimeUtc { get; }


        [JsonProperty("startTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime StartTimeUtc { get; }


        [JsonProperty]
        public int TeamSize { get; }


        [JsonProperty]
        internal WarClan? Clan { get; }

        [JsonProperty]
        internal WarClan? Opponent { get; }


        [JsonProperty]
        public WarState State { get; }


        [JsonProperty]
        public IList<WarClan> Clans { get; internal set; } = new List<WarClan>();

        [JsonProperty]
        public List<Attack> Attacks { get; internal set; } = new List<Attack>();


        [JsonProperty]
        public string WarId { get; private set; } = string.Empty;

        [JsonProperty]
        public WarType WarType { get; internal set; } = WarType.Random;

        [JsonProperty]
        public DateTime WarEndingSoonUtc { get; internal set; }

        [JsonProperty]
        public DateTime WarStartingSoonUtc { get; internal set; }

        [JsonProperty]
        public CurrentWarFlags Flags { get; internal set; } = new CurrentWarFlags();

        public void Initialize()
        {
            SetWarEndingSoonWarning();

            SetWarStartingSoonWarning();

            if (Clan != null) Clans.Add(Clan);

            if (Opponent != null) Clans.Add(Opponent);

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

            if (this is LeagueWar)
            {
                WarType = WarType.SCCWL;
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

            foreach (WarClan clan in Clans)
            {
                clan.WarId = WarId;

                foreach (WarVillage warVillage in clan.Villages.EmptyIfNull())
                {
                    warVillage.WarClanId = clan.WarClanId;

                    warVillage.ClanTag = clan.ClanTag;

                    warVillage.WarId = WarId;

                    foreach (Attack attack in warVillage.Attacks.EmptyIfNull())
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

                if (attacksThisBase.Count == 0)
                {
                    attack.StarsGained = attack.Stars;
                }
                else
                {
                    attack.StarsGained = Math.Max(attack.Stars - attacksThisBase.OrderBy(a => a.Stars).First().Stars, 0);                    
                }

                foreach (var clan in Clans)
                {
                    WarVillage? attacker = clan.Villages.FirstOrDefault(m => m.VillageTag == attack.AttackerTag);

                    if (attacker != null)
                    {
                        attack.AttackerClanTag = clan.ClanTag;

                        attack.AttackerMapPosition = attacker.MapPosition;

                        attack.AttackerTownHallLevel = attacker.TownhallLevel;
                    }

                    WarVillage? defender = clan.Villages.FirstOrDefault(m => m.VillageTag == attack.DefenderTag);

                    if (defender != null)
                    {
                        attack.DefenderClanTag = clan.ClanTag;

                        attack.DefenderMapPosition = defender.MapPosition;

                        attack.DefenderTownHallLevel = defender.TownhallLevel;
                    }
                }
            }

            foreach (WarClan clan in Clans)
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

            if (Clans[0].Villages.All(m => m.Attacks?.Count == 2) && Clans[1].Villages.All(m => m.Attacks?.Count == 2)) return true;

            return false;
        }

        internal void Update(CocApi cocApi, IWar? downloadedWar, ILeagueGroup? leagueGroupApiModel)
        {
            SendWarNotifications(cocApi, downloadedWar);

            if (ReferenceEquals(this, downloadedWar))
            {
                return;
            }

            if (downloadedWar is CurrentWar currentWar)
            {
                //the type of war should only be decided on the wars fist download
                //to prevent maintenance breaks from changing the type to random
                currentWar.WarType = this.WarType;
            }

            UpdateWar(cocApi, downloadedWar);

            UpdateAttacks(cocApi, downloadedWar);

            UpdateLeagueTeamSize(cocApi, leagueGroupApiModel);
        }

        private void SendWarNotifications(CocApi cocApi, IWar? downloadedWar)
        {
            IActiveWar? currentWar = downloadedWar as IActiveWar;

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
            if (!(downloadedWar is IActiveWar currentWar) || currentWar.WarId != WarId) return;

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
            if (!(downloadedWar is IActiveWar currentWar) || currentWar.WarId != WarId) return;

            List<Attack> newAttacks = currentWar.Attacks.Where(a => a.Order > Attacks.Count).ToList();

            cocApi.NewAttacksEvent(currentWar, newAttacks);
        }

        private void UpdateLeagueTeamSize(CocApi cocApi, ILeagueGroup? leagueGroup)
        {
            if (leagueGroup is LeagueGroup leagueGroupApiModel)
            {
                if (leagueGroupApiModel.TeamSize > 15) return;

                if (Clans.Any(c => c.AttackCount > 15))
                {
                    leagueGroupApiModel.TeamSize = 30;

                    cocApi.LeagueGroupTeamSizeChangedEvent(leagueGroupApiModel);
                }
            }            
        }
    }
}
