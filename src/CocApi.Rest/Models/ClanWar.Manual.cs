using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

            return stored.WarClan.Tag == fetched.WarClan.Tag;
        }

        public static string Url(string clanTag)
        {
            if (Clash.TryFormatTag(clanTag, out string? formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar";
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public string? WarTag { get; internal set; }

        /// <summary>
        /// Gets both clans in the war, sorted by tag in ascending order.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public WarClan[] WarClans => Clan?.Tag != null && Opponent?.Tag != null
            ? new[] { Clan, Opponent }.OrderBy(c => c.Tag).ToArray()
            : [];

        /// <summary>
        /// Gets the clan with the lower tag (first in sorted order).
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public WarClan WarClan => Clan.Tag != null && Opponent.Tag != null
            ? (string.Compare(Clan.Tag, Opponent.Tag) < 0 ? Clan : Opponent)
            : Clan;

        /// <summary>
        /// Gets the clan with the higher tag (second in sorted order).
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public WarClan WarOpponent => Clan.Tag != null && Opponent.Tag != null
            ? (string.Compare(Clan.Tag, Opponent.Tag) < 0 ? Opponent : Clan)
            : Opponent;

        [System.Text.Json.Serialization.JsonIgnore]
        public List<ClanWarAttack> Attacks { get; private set; } = new List<ClanWarAttack>();

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

                foreach (WarClan warClan in WarClans)
                    foreach (ClanWarMember member in warClan.Members)
                        foreach (ClanWarAttack attack in member.Attacks.EmptyIfNull())
                            Attacks.Add(attack);

                foreach (WarClan warClan in WarClans)
                {
                    debug = "e";
                    if (AllAttacksAreUsedOrWarIsOver())
                    {
                        debug = "f";
                        WarClan enemy = WarClans.First(c => c.Tag != warClan.Tag);

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
                    int warPosition = 1;

                    foreach (ClanWarMember member in warClan.Members.OrderBy(m => m.RosterPosition))
                    {
                        debug = "i";
                        member.WarPosition = warPosition;
                        warPosition++;

                        foreach (ClanWarAttack attack in member.Attacks.EmptyIfNull())
                        {
                            debug = "j";
                            WarClan defendingClan = WarClans.First(c => c.Tag != warClan.Tag);

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
                foreach (var wc in WarClans)
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
