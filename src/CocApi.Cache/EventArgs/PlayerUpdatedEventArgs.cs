using CocApi.Model;
using System;
using System.Threading;

namespace CocApi.Cache
{
    public class PlayerUpdatedEventArgs : CancellableEventArgs
    {
        public Player Fetched { get; }

        public Player? Stored { get; }

        public PlayerUpdatedEventArgs(Player? stored, Player fetched, CancellationToken cancellationtoken) : base(cancellationtoken)
        {
            Fetched = fetched;

            Stored = stored;
        }
    }
}
