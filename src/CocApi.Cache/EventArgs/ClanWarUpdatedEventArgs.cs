using CocApi.Model;
using System;

namespace CocApi.Cache
{
    public class ClanWarUpdatedEventArgs : EventArgs
    {
        public ClanWar Fetched { get; }

        public ClanWar Stored { get; }

        public Clan Clan { get; }

        public ClanWarUpdatedEventArgs(Model.Clan clan, ClanWar stored, ClanWar fetched)
        {
            Clan = clan;

            Fetched = fetched;

            Stored = stored;
        }
    }
}
