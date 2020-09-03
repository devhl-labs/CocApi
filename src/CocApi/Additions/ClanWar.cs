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
            return other != null &&
                   PreparationStartTime == other.PreparationStartTime &&
                   Clans.First().Key == other.Clans.First().Key;
        }

        [DataMember(Name = "type", EmitDefaultValue = false)]
        public WarType Type { get; set; }


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
        }
    }
}
