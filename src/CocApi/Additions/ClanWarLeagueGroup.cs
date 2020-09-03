using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanWarLeagueGroup : IEquatable<ClanWarLeagueGroup?>
    {
        public static string Url(string clanTag)
        {
            if (Clash.TryFormatTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar/leaguegroup";
        }

        public static string WarTag(string url)
        {
            url = url.Replace("clans/", "").Replace("/currentwar/leaguegroup", "");

            return Uri.UnescapeDataString(url);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ClanWarLeagueGroup);
        }

        public bool Equals(ClanWarLeagueGroup? other)
        {
            return other != null &&
                   Season == other.Season &&
                   Clans.OrderBy(c => c.Tag).First().Tag == other.Clans.OrderBy(c => c.Tag).First().Tag;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Clans.OrderBy(c => c.Tag).First().Tag, Season);
        }
    }
}
