using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanWar
    {
        public static string Url(string clanTag)
        {
            if (Clash.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar";
        }
    }
}
