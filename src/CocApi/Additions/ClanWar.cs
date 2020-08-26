using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanWar
    {
        public static string Url(string clanTag)
        {
            if (Clash.TryFormatTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar";
        }

        public string WarTag { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(PreparationStartTime, Clans.Values.First().Tag, Clans.Values.Skip(1).First().Tag);
        }

        private SortedDictionary<string, WarClan> _clans;
        
        public SortedDictionary<string, WarClan> Clans
        {
            get
            {
                if (_clans != null)
                    return _clans;

                if (Clan == null || Clan.Tag == null || Opponent == null || Opponent.Tag == null)
                    return null;

                _clans = new SortedDictionary<string, WarClan>
                {
                    { Clan.Tag, Clan },

                    { Opponent.Tag, Opponent }
                };

                return _clans;
            }
        }

        private readonly List<ClanWarAttack> _allAttacks = new List<ClanWarAttack>();

        public List<ClanWarAttack> AllAttacks
        {
            get
            {
                if (_allAttacks.Count == 0)
                    foreach (WarClan warClan in Clans.Values)
                        foreach (ClanWarMember member in warClan.Members)
                            foreach (ClanWarAttack attack in member.Attacks.EmptyIfNull())
                                _allAttacks.Add(attack);

                return _allAttacks;
            }
        }

        public bool HasWarUpdated(ClanWar clanWar)
        {
            if (IsSameWar(clanWar) == false)
                throw new ArgumentException();

            if (Clans.First().Value.Attacks != clanWar.Clans.First().Value.Attacks)
                return true;

            if (Clans.Skip(1).First().Value.Attacks != clanWar.Clans.Skip(1).First().Value.Attacks)
                return true;

            if (EndTime != clanWar.EndTime)
                return true;

            if (StartTime != clanWar.StartTime)
                return true;

            if (State != clanWar.State)
                return true;

            return false;
        }

        public bool IsSameWar(ClanWar clanWar)
        {
            if (ReferenceEquals(this, clanWar))
                return true;

            if (PreparationStartTime != clanWar.PreparationStartTime)
                return false;

            if (Clan.Tag == clanWar.Clan.Tag)
                return true;

            if (Clan.Tag == clanWar.Opponent.Tag)
                return true;

            return false;
        }

        public static List<ClanWarAttack> NewAttacks(ClanWar stored, ClanWar fetched)
        {
            List<ClanWarAttack> attacks = new List<ClanWarAttack>();

            foreach (ClanWarAttack attack in fetched.AllAttacks)
                if (stored.AllAttacks.Count(a => a.AttackerTag == attack.AttackerTag && a.DefenderTag == attack.DefenderTag) == 0)
                    attacks.Add(attack);

            return attacks;
        }

        public List<ClanWarAttack> NewAttacks(ClanWar fetched) => NewAttacks(this, fetched);
    }
}
