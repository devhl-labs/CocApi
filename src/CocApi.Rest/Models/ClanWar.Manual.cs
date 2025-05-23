using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

// TODO: probably dont need this anymore
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
                return false;

            if (stored.Clan.Tag == fetched.Clan.Tag)
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

        private volatile SortedDictionary<string, WarClan>? _clans;

        private readonly object _clansLock = new();

        public SortedDictionary<string, WarClan> Clans
        {
            get
            {
                if (_clans != null) // avoid the lock if we can
                    return _clans;

                lock (_clansLock)
                {
                    if (_clans != null)
                        return _clans;

                    _clans = (Clan?.Tag == null || Opponent?.Tag == null)
                        ? new SortedDictionary<string, WarClan>()
                        : new SortedDictionary<string, WarClan>
                            {
                                { Clan.Tag, Clan },
                                { Opponent.Tag, Opponent }
                            };

                    return _clans;
                }
            }
        }

        private volatile List<ClanWarAttack>? _attacks;
        private readonly object _attacksLock = new();

        public List<ClanWarAttack> Attacks
        {
            get
            {
                if (_attacks != null) // avoid the lock if we can
                    return _attacks;

                lock (_attacksLock)
                {
                    if (_attacks != null)
                        return _attacks;

                    _attacks = new List<ClanWarAttack>();

                    foreach (WarClan warClan in Clans.Values)
                        foreach (ClanWarMember member in warClan.Members)
                            foreach (ClanWarAttack attack in member.Attacks.EmptyIfNull())
                                _attacks.Add(attack);

                    return _attacks;
                }
            }
        }

        public static List<ClanWarAttack> NewAttacks(ClanWar stored, ClanWar fetched)
        {
            List<ClanWarAttack> attacks = new();

            foreach (ClanWarAttack attack in fetched.Attacks)
                if (!stored.Attacks.Any(a => a.AttackerTag == attack.AttackerTag && a.DefenderTag == attack.DefenderTag))
                    attacks.Add(attack);

            return attacks;
        }

        public List<ClanWarAttack> NewAttacks(ClanWar fetched) => NewAttacks(this, fetched);

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

        partial void OnCreated()
        {
            string debug = "a";
            try
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

                debug = "b";

                Clan = sorted.Values.First();
                debug = "c";
                Opponent = sorted.Values.Skip(1).First();
                debug = "d";
                foreach (WarClan warClan in Clans.Values)
                {
                    debug = "e";
                    if (AllAttacksAreUsedOrWarIsOver())
                    {
                        debug = "f";
                        WarClan enemy = Clans.First(c => c.Key != warClan.Tag).Value;

                        debug = "g";
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

                    debug = "h";
                    int mapPosition = 1;

                    foreach (ClanWarMember member in warClan.Members.OrderBy(m => m.RosterPosition))
                    {
                        debug = "i";
                        member.MapPosition = mapPosition;
                        mapPosition++;

                        foreach (ClanWarAttack attack in member.Attacks.EmptyIfNull())
                        {
                            debug = "j";
                            WarClan defendingClan = Clans.First(c => c.Key != warClan.Tag).Value;

                            debug = "k";
                            ClanWarMember defending = defendingClan.Members.First(m => m.Tag == attack.DefenderTag);

                            debug = "l";
                            attack.AttackerClanTag = warClan.Tag;
                            attack.DefenderClanTag = defendingClan.Tag;
                            attack.AttackerTownHall = member.TownhallLevel;
                            attack.DefenderTownHall = defending.TownhallLevel;
                            attack.AttackerMapPosition = member.RosterPosition;
                            attack.DefenderMapPosition = defending.RosterPosition;
                        }
                    }
                }

                debug = "m";
                foreach (var wc in Clans.Values)
                {
                    debug = "n";
                    var grouped = Attacks.Where(a => a.AttackerClanTag == wc.Tag).GroupBy(a => a.DefenderMapPosition);
                    debug = "o";
                    foreach (var group in grouped)
                    {
                        debug = "p";
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
                    debug = "q";
                }

                debug = "r";
                // cwl does not include this property
                if (AttacksPerMember == 0)
                    AttacksPerMember = 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed ClanWar#OnCreated at: {debug} exception: ${e.Message}");
                throw;
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
