using System;
using System.Collections.Generic;
using System.Linq;
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

        

    }
}
