using CocApi.Cache.Context.CachedItems;
using CocApi.Model;
using System;

namespace CocApi.Cache
{
    public class WarEventArgs : EventArgs
    {
        public ClanWar War {get;}

        public WarEventArgs(ClanWar war)
        {
            War = war;
        }
    }
}
