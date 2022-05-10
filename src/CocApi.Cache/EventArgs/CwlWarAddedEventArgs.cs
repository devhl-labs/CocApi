using System.Threading;
using CocApi.Rest.Models;

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
