using CocApi.Model;
using System;

namespace CocApi.Cache
{

    public class ClanWarLeagueGroupUpdatedEventArgs : EventArgs
    {
        public ClanWarLeagueGroup Fetched { get; }

        public ClanWarLeagueGroup? Stored { get; }

        public Clan Clan { get; }

        public ClanWarLeagueGroupUpdatedEventArgs(Model.Clan clan, ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched)
        {
            Fetched = fetched;

            Stored = stored;

            Clan = clan;
        }
    }
}
