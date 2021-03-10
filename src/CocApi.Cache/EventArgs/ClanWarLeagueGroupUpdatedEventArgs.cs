using CocApi.Model;
using System;
using System.Threading;

namespace CocApi.Cache
{

    public class ClanWarLeagueGroupUpdatedEventArgs : CancellableEventArgs
    {
        public ClanWarLeagueGroup Fetched { get; }

        public ClanWarLeagueGroup? Stored { get; }

        public Clan? Clan { get; }

        internal ClanWarLeagueGroupUpdatedEventArgs(ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched, Clan? clan, CancellationToken cancellationToken) : base(cancellationToken)
        {
            Fetched = fetched;
            Clan = clan;
            Stored = stored;
        }
    }
}
