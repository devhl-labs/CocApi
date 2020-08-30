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

        private SortedDictionary<string, WarClan> _clans = new SortedDictionary<string, WarClan>();
        
        public SortedDictionary<string, WarClan> Clans
        {
            get
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

        private readonly List<ClanWarAttack> _allAttacks = new List<ClanWarAttack>();

        public List<ClanWarAttack> AllAttacks
        {
            get
            {
                if (Clans.Count == 0)
                    return _allAttacks;

                if (_allAttacks.Count == 0)
                    foreach (WarClan warClan in Clans.Values)
                        foreach (ClanWarMember member in warClan.Members)
                            foreach (ClanWarAttack attack in member.Attacks.EmptyIfNull())
                                _allAttacks.Add(attack);

                return _allAttacks;
            }
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

        public enum TypeEnum
        {
            Unknown,
            Random,
            Friendly,
            SCCWL
        }

        public TypeEnum Type { get; set; }
    }
}
