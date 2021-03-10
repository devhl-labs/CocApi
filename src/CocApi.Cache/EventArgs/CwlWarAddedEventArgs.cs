using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Model;

namespace CocApi.Cache
{
    public class CwlWarAddedEventArgs : WarAddedEventArgs
    {
        internal CwlWarAddedEventArgs(Clan? clan, Clan? opponent, ClanWar war, ClanWarLeagueGroup group, CancellationToken cancellationToken) : base(clan, opponent, war, cancellationToken)
        {
            Group = group;
        }

        public ClanWarLeagueGroup Group { get; }
    }
}
