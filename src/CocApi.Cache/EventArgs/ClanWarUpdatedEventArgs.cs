using CocApi.Model;
using System;

namespace CocApi.Cache
{
    public class ClanWarUpdatedEventArgs : EventArgs
    {
        public ClanWar Fetched { get; }

        public ClanWar Stored { get; }

        public ClanWarUpdatedEventArgs(ClanWar stored, ClanWar fetched)
        {
            Fetched = fetched;

            Stored = stored;
        }
    }
}
