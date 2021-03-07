using CocApi.Cache.Context.CachedItems;
using CocApi.Model;
using System;

namespace CocApi.Cache
{
    public class CwlWarAddedEventArgs : WarAddedEventArgs
    {
        public CwlWarAddedEventArgs(Clan? clan, Clan? opponent, ClanWar war, ClanWarLeagueGroup group) : base(clan, opponent, war)
        {
            Group = group;
        }

        public ClanWarLeagueGroup Group { get; }
    }

    public class WarAddedEventArgs : EventArgs
    {
        public Clan? Clan { get; }

        public Clan? Opponent { get; }

        public ClanWar War { get; }

        public WarAddedEventArgs(Clan? clan, Clan? opponent, ClanWar war)
        {
            Clan = clan;
            Opponent = opponent;
            War = war;
        }
    }
}
