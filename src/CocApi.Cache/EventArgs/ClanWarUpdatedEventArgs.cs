using CocApi.Rest.Models;
using System.Threading;

namespace CocApi.Cache
{
    public class ClanWarUpdatedEventArgs : CancellableEventArgs
    {
        public ClanWar Fetched { get; }
        public Clan? Clan { get; }
        public Clan? Opponent { get; }
        public ClanWar Stored { get; }

        internal ClanWarUpdatedEventArgs(ClanWar stored, ClanWar fetched, Clan? cachedClan, Clan? cachedOpponent, CancellationToken cancellationToken) : base(cancellationToken)
        {
            Fetched = fetched;
            Clan = cachedClan;
            Opponent = cachedOpponent;
            Stored = stored;
        }
    }
}
