using CocApi.Model;
using System;

namespace CocApi.Cache
{
    public class PlayerUpdatedEventArgs : EventArgs
    {
        public Player Fetched { get; }

        public Player? Stored { get; }

        public PlayerUpdatedEventArgs(Player? stored, Player fetched)
        {
            Fetched = fetched;

            Stored = stored;
        }
    }
}
