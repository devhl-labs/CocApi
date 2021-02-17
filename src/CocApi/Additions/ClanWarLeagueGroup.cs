using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanWarLeagueGroup : IEquatable<ClanWarLeagueGroup?>
    {
        public static string Url(string clanTag)
        {
            if (Clash.TryFormatTag(clanTag, out string? formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar/leaguegroup";
        }

        public static string WarTag(string url)
        {
            url = url.Replace("clans/", "").Replace("/currentwar/leaguegroup", "");

            return Uri.UnescapeDataString(url);
        }

        //public override bool Equals(object? obj)
        //{
        //    return Equals(obj as ClanWarLeagueGroup);
        //}

        //public bool Equals(ClanWarLeagueGroup? other)
        //{
        //    return other != null &&
        //           Season == other.Season &&
        //           Clans.OrderBy(c => c.Tag).First().Tag == other.Clans.OrderBy(c => c.Tag).First().Tag;
        //}

        //public override int GetHashCode()
        //{
        //    return HashCode.Combine(Clans.OrderBy(c => c.Tag).First().Tag, Season);
        //}

        ///// <summary>
        ///// Gets or Sets State
        ///// </summary>
        //[DataMember(Name = "state", EmitDefaultValue = false)]
        //public GroupState? State { get; private set; }


        ///// <summary>
        ///// Initializes a new instance of the <see cref="ClanWarLeagueGroup" /> class.
        ///// </summary>
        ///// <param name="tag">tag.</param>
        ///// <param name="state">state.</param>
        ///// <param name="season">season.</param>
        ///// <param name="clans">clans.</param>
        ///// <param name="rounds">rounds.</param>
        //public ClanWarLeagueGroup(string tag = default(string), GroupState? state = default(GroupState?), DateTime season = default(DateTime), List<ClanWarLeagueClan> clans = default(List<ClanWarLeagueClan>), List<ClanWarLeagueRound> rounds = default(List<ClanWarLeagueRound>))
        //{
        //    this.Tag = tag;
        //    this.State = state;
        //    this.Season = season;
        //    this.Clans = clans;
        //    this.Rounds = rounds;
        //}
    }
}
