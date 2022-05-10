using CocApi.Rest.Models;
using System.Threading;

namespace CocApi.Cache
{
    public class PlayerUpdatedEventArgs : CancellableEventArgs
    {
        public Player Fetched { get; }

        public Player? Stored { get; }

        internal PlayerUpdatedEventArgs(Player? stored, Player fetched, CancellationToken cancellationtoken) : base(cancellationtoken)
        {
            Fetched = fetched;

            Stored = stored;
        }
    }
}
