using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("CocApi.Cache")]
namespace CocApi.Rest.Models
{
    public partial class ClanWar : IEquatable<ClanWar?>
    {
        public static bool IsSameWar(ClanWar? stored, ClanWar? fetched)
        {
            if (ReferenceEquals(stored, fetched))
                return true;
            if (stored == null || fetched == null)
                return true;
            if (stored.PreparationStartTime != fetched.PreparationStartTime)
                return false;            if (stored.Clan.Tag == fetched.Clan.Tag)
                return true;
            if (stored.Clan.Tag == fetched.Opponent.Tag)
                return true;
            return false;
        }
        public static string Url(string clanTag)
        {
            if (Clash.TryFormatTag(clanTag, out string? formattedTag) == false)
                throw new InvalidTagException(clanTag);
            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar";
        }
        [JsonPropertyName("warTag")]
        public string? WarTag { get; internal set; }

        private SortedDictionary<string, WarClan> _clans = new();
        private bool _isInitialized;
        private readonly object _clansLock = new();
        public SortedDictionary<string, WarClan> Clans
        {
            get
            {
                lock (_clansLock)
                {
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
        }
        private List<ClanWarAttack> _attacks = new();
        private readonly object _attacksLock = new();
        public List<ClanWarAttack> Attacks
        {
            get
            {
                lock (_attacksLock)
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
            }
            internal set
            {
                lock (_attacksLock)
                    _attacks = value ?? new List<ClanWarAttack>();
            }
        }

        public static List<ClanWarAttack> NewAttacks(ClanWar stored, ClanWar fetched)
        {
            List<ClanWarAttack> attacks = new();

            foreach (ClanWarAttack attack in fetched.Attacks)
                if (stored.Attacks.Count(a => a.AttackerTag == attack.AttackerTag && a.DefenderTag == attack.DefenderTag) == 0)
                    attacks.Add(attack);

            return attacks;
        }

        public List<ClanWarAttack> NewAttacks(ClanWar fetched) => NewAttacks(this, fetched);
        [JsonPropertyName("serverExpiration")]
        public DateTime ServerExpiration { get; internal set; }
        public bool AllAttacksAreUsed()
        {
            int totalAttacks = Clan.Members.Count + Opponent.Members.Count;

            if (WarTag == null)
                totalAttacks *= 2;

            return Attacks.Count == totalAttacks;
        }

        public bool AllAttacksAreUsedOrWarIsOver()
        {
            if (State == WarState.WarEnded)
                return true;
            return AllAttacksAreUsed();
        }

        private readonly object _initializeLock = new();
        internal void Initialize(DateTime serverExpiration, string? warTag)
        {
            lock (_initializeLock)
            {
                if (_isInitialized)
                    return;

                ServerExpiration = serverExpiration;
                WarTag = warTag;
                _isInitialized = true;
            }
        }

        private void Initialize()
        {
            if (State == WarState.NotInWar)
                return;

            WarClan clan = Clan;
            WarClan opponent = Opponent;
            SortedList<string, WarClan> sorted = new SortedList<string, WarClan>
            {
                { clan.Tag, clan },
                { opponent.Tag, opponent }
            };
            Clan = sorted.Values.First();
            Opponent = sorted.Values.Skip(1).First();

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
            foreach (var wc in Clans.Values)
            {
                var grouped = Attacks.Where(a => a.AttackerClanTag == wc.Tag).GroupBy(a => a.DefenderMapPosition);
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
        }

        public WarType GetWarType()
        {
            if (WarTag != null)
                return WarType.SCCWL;

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
                || timeSpan.TotalMinutes == 15
                || timeSpan.TotalMinutes == 5)
                return WarType.Friendly;

            return WarType.Random;
        }
    }
}