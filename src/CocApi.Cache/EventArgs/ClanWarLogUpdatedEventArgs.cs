using CocApi.Cache.Context.CachedItems;
using CocApi.Model;
using System;

namespace CocApi.Cache
{
    public class ClanWarLogUpdatedEventArgs : EventArgs
    {
        public ClanWarLog Fetched { get; }

        public ClanWarLog? Stored { get; }

        public Clan? Clan { get; }

        public ClanWarLogUpdatedEventArgs(ClanWarLog? stored, ClanWarLog fetched, Clan? clan)
        {
            Fetched = fetched;
            Clan = clan;
            Stored = stored;
        }
    }
}
