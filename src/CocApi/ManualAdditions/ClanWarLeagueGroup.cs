using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanWarLeagueGroup
    {
        public static string Url(string clanTag)
        {
            if (Clash.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar/leaguegroup";
        }

        public static string WarTag(string url)
        {
            url = url.Replace("clans/", "").Replace("/currentwar/leaguegroup", "");

            return Uri.UnescapeDataString(url);
        }

        [NotMapped]
        public DateTime SeasonDate
        {
            get
            {
                return DateTime.ParseExact(Season, "yyyy'-'MM", CultureInfo.InvariantCulture);
            }
        }
    }
}
