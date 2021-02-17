using CocApi.Cache.Context.CachedItems;
using CocApi.Model;
using System;

namespace CocApi.Cache
{
    public class WarAddedEventArgs : EventArgs
    {
        public Clan? Clan { get; set; }

        public ClanWar War { get; set; }

        public WarAddedEventArgs(Clan? clan, ClanWar war)
        {
            Clan = clan;

            War = war;
        }
    }
}
