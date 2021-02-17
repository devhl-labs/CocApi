using CocApi.Cache.Context.CachedItems;
using CocApi.Model;
using System;

namespace CocApi.Cache
{
    public class ClanUpdatedEventArgs : EventArgs
    {
        public Clan Fetched { get; }

        public Clan? Stored { get; }

        public ClanUpdatedEventArgs(Clan? stored, Clan fetched)
        {
            Fetched = fetched;

            Stored = stored;
        }
    }
}
