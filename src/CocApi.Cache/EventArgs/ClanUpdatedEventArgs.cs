using CocApi.Rest.Models;
using System.Threading;

namespace CocApi.Cache;

public class ClanUpdatedEventArgs : CancellableEventArgs
{
    public Clan Fetched { get; }

    public Clan? Stored { get; }

    internal ClanUpdatedEventArgs(Clan? stored, Clan fetched, CancellationToken cancellationToken) : base(cancellationToken)
    {
        Fetched = fetched;

        Stored = stored;
    }
}
