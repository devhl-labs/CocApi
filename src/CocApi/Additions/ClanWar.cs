using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace CocApi.Model
{
    public partial class ClanWar : IEquatable<ClanWar?>
    {
        public static bool IsSameWar(ClanWar? stored, ClanWar fetched)
        {
            if (ReferenceEquals(stored, fetched))
                return true;

            if (stored == null)
                return true;

            if (stored.PreparationStartTime != fetched.PreparationStartTime)
                return false;

            if (stored.Clan.Tag == fetched.Clan.Tag)
                return true;

            if (stored.Clan.Tag == fetched.Opponent.Tag)
                return true;

            return false;
        }

        public static string Url(string clanTag)
        {
            if (Clash.TryFormatTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar";
        }

        [DataMember(Name = "warTag", EmitDefaultValue = false)]
        public string? WarTag { get; internal set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(PreparationStartTime, Clans.Values.First().Tag);
        }

        private SortedDictionary<string, WarClan> _clans = new SortedDictionary<string, WarClan>();

        private bool _isInitialized;

        public SortedDictionary<string, WarClan> Clans
        {
            get
            {
                //Initialize();

                if (_clans.Count > 0)
                    return _clans;

                if (Clan == null || Clan.Tag == null || Opponent == null || Opponent.Tag == null)
                    return _clans;

                _clans = new SortedDictionary<string, WarClan>
                {
                    { Clan.Tag, Clan },

                    { Opponent.Tag, Opponent }
                };

                return _clans;
            }
        }

        private List<ClanWarAttack> _attacks = new List<ClanWarAttack>();

        public List<ClanWarAttack> Attacks
        {
            get
            {
                if (Clans.Count == 0)
                    return _attacks;

                if (_attacks.Count == 0)
                    foreach (WarClan warClan in Clans.Values)
                        foreach (ClanWarMember member in warClan.Members)
                            foreach (ClanWarAttack attack in member.Attacks.EmptyIfNull())
                                _attacks.Add(attack);

                return _attacks;
            }

            internal set
            {
                _attacks = value ?? new List<ClanWarAttack>();
            }
        }

        public static List<ClanWarAttack> NewAttacks(ClanWar stored, ClanWar fetched)
        {
            List<ClanWarAttack> attacks = new List<ClanWarAttack>();

            foreach (ClanWarAttack attack in fetched.Attacks)
                if (stored.Attacks.Count(a => a.AttackerTag == attack.AttackerTag && a.DefenderTag == attack.DefenderTag) == 0)
                    attacks.Add(attack);

            return attacks;
        }

        public List<ClanWarAttack> NewAttacks(ClanWar fetched) => NewAttacks(this, fetched);

        public override bool Equals(object? obj)
        {
            return Equals(obj as ClanWar);
        }

        public bool Equals(ClanWar? other)
        {
            //Initialize();

            //other?.Initialize();

            return other != null &&
                   PreparationStartTime == other.PreparationStartTime &&
                   Clans.First().Key == other.Clans.First().Key;
        }

        [DataMember(Name = "type", EmitDefaultValue = false)]
        public WarType WarType { get; set; }

        [DataMember(Name = "serverResponseExpires", EmitDefaultValue = false)]
        public DateTime ServerResponseExpires { get; internal set; }

        public bool AllAttacksAreUsed()
        {
            int totalAttacks = TeamSize * 2;

            if (WarTag == null)
                totalAttacks *= 2;

            if (Attacks.Count == totalAttacks)
                return true;

            return false;
        }

        public bool AllAttacksAreUsedOrWarIsOver()
        {
            if (State == WarState.WarEnded)
                return true;

            return AllAttacksAreUsed();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClanWar" /> class.
        /// </summary>
        /// <param name="clan">clan.</param>
        /// <param name="teamSize">teamSize.</param>
        /// <param name="opponent">opponent.</param>
        /// <param name="startTime">startTime.</param>
        /// <param name="state">state.</param>
        /// <param name="endTime">endTime.</param>
        /// <param name="preparationStartTime">preparationStartTime.</param>
        [JsonConstructor]
        public ClanWar(List<ClanWarAttack> allAttacks, WarClan clan = default(WarClan), int teamSize = default(int), WarClan opponent = default(WarClan), DateTime startTime = default(DateTime), WarState? state = default(WarState?), DateTime endTime = default(DateTime), DateTime preparationStartTime = default(DateTime))
        {
            this.Attacks = allAttacks;
            this.Clan = clan;
            this.TeamSize = teamSize;
            this.Opponent = opponent;
            this.StartTime = startTime;
            this.State = state;
            this.EndTime = endTime;
            this.PreparationStartTime = preparationStartTime;

            //Initialize();
        }

        private readonly object _initializeLock = new object();

        public void Initialize()
        {
            lock (_initializeLock)
            {
                if (_isInitialized)
                    return;

                if (State == WarState.NotInWar)
                    return;

                foreach (WarClan warClan in Clans.Values)
                {
                    if (AllAttacksAreUsedOrWarIsOver())
                    {
                        WarClan enemy = Clans.First(c => c.Key != warClan.Tag).Value;

                        if (warClan.Stars > enemy.Stars)
                            warClan.Result = Result.Win;
                        else if (warClan.Stars < enemy.Stars)
                            warClan.Result = Result.Lose;
                        else if (warClan.Stars == enemy.Stars && warClan.DestructionPercentage > enemy.DestructionPercentage)
                            warClan.Result = Result.Win;
                        else if (warClan.Stars == enemy.Stars && warClan.DestructionPercentage < enemy.DestructionPercentage)
                            warClan.Result = Result.Lose;
                        else
                            warClan.Result = Result.Tie;
                    }

                    int mapPosition = 1;

                    foreach (ClanWarMember member in warClan.Members.OrderBy(m => m.RosterPosition))
                    {
                        member.MapPosition = mapPosition;
                        mapPosition++;

                        foreach (ClanWarAttack attack in member.Attacks.EmptyIfNull())
                        {
                            WarClan defendingClan = Clans.First(c => c.Key != warClan.Tag).Value;
                            ClanWarMember defending = defendingClan.Members.First(m => m.Tag == attack.DefenderTag);

                            attack.AttackerClanTag = warClan.Tag;
                            attack.DefenderClanTag = defendingClan.Tag;
                            attack.AttackerTownHall = member.TownhallLevel;
                            attack.DefenderTownHall = defending.TownhallLevel;
                            attack.AttackerMapPosition = member.RosterPosition;
                            attack.DefenderMapPosition = defending.RosterPosition;
                        }
                    }
                }


                foreach (var clan in Clans.Values)
                {
                    var grouped = Attacks.Where(a => a.AttackerClanTag == clan.Tag).GroupBy(a => a.DefenderMapPosition);

                    foreach (var group in grouped)
                    {
                        bool fresh = true;
                        int maxStars = 0;

                        foreach (var attack in group.OrderBy(g => g.Order))
                        {
                            attack.Fresh = fresh;
                            fresh = false;

                            attack.StarsGained = attack.Stars - maxStars;

                            if (attack.StarsGained < 0)
                                attack.StarsGained = 0;

                            if (attack.Stars > maxStars)
                                maxStars = attack.Stars;
                        }
                    }
                }

                TimeSpan timeSpan = StartTime - PreparationStartTime;

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

                if (timeSpan.TotalHours == 23)
                    WarType = WarType.Random;

                if (WarTag != null)
                    WarType = WarType.SCCWL;

                _isInitialized = true;
            }           
        }
    }
}
