using CocApi.Model;
using System;

namespace CocApi.Cache
{
    public class ClanWarLogUpdatedEventArgs : EventArgs
    {
        public ClanWarLog Fetched { get; }

        public ClanWarLog? Stored { get; }

        public ClanWarLogUpdatedEventArgs(ClanWarLog? stored, ClanWarLog fetched)
        {
            Fetched = fetched;

            Stored = stored;
        }
    }
}
