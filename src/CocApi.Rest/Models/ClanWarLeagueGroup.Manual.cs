using System;

namespace CocApi.Rest.Models
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
    }
}
