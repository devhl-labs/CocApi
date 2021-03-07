using CocApi.Model;
using System;

namespace CocApi.Cache
{

    public class ClanWarLeagueGroupUpdatedEventArgs : EventArgs
    {
        public ClanWarLeagueGroup Fetched { get; }

        public ClanWarLeagueGroup? Stored { get; }

        public Clan? Clan { get; }

        public ClanWarLeagueGroupUpdatedEventArgs(ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched, Clan? clan)
        {
            Fetched = fetched;
            Clan = clan;
            Stored = stored;
        }
    }
}
