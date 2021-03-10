using CocApi.Cache.Context.CachedItems;
using CocApi.Model;
using System;
using System.Threading;

namespace CocApi.Cache
{
    public class ClanUpdatedEventArgs : CancellableEventArgs
    {
        public Clan Fetched { get; }

        public Clan? Stored { get; }

        public ClanUpdatedEventArgs(Clan? stored, Clan fetched, CancellationToken cancellationToken) : base(cancellationToken)
        {
            Fetched = fetched;

            Stored = stored;
        }
    }
}
