using CocApi.Model;
using System;

namespace CocApi.Cache
{

    public class ClanWarLeagueGroupUpdatedEventArgs : EventArgs
    {
        public ClanWarLeagueGroup Fetched { get; }

        public ClanWarLeagueGroup? Stored { get; }

        public ClanWarLeagueGroupUpdatedEventArgs(ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched)
        {
            Fetched = fetched;

            Stored = stored;
        }
    }
}
