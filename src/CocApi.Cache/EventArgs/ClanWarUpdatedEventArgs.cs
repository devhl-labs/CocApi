using CocApi.Cache.Context.CachedItems;
using CocApi.Model;
using System;
using System.Collections.Generic;

namespace CocApi.Cache
{
    public class ClanWarUpdatedEventArgs : EventArgs
    {
        public ClanWar Fetched { get; }
        public Clan? Clan { get; }
        public Clan? Opponent { get; }
        public ClanWar Stored { get; }

        public ClanWarUpdatedEventArgs(ClanWar stored, ClanWar fetched, Clan? cachedClan, Clan? cachedOpponent)
        {
            Fetched = fetched;
            Clan = cachedClan;
            Opponent = cachedOpponent;
            Stored = stored;
        }
    }
}
