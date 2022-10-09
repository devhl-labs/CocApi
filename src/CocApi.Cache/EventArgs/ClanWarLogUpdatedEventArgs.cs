using CocApi.Rest.Models;
using System.Threading;

namespace CocApi.Cache;

public class ClanWarLogUpdatedEventArgs : CancellableEventArgs
{
    public ClanWarLog Fetched { get; }

    public ClanWarLog? Stored { get; }

    public Clan? Clan { get; }

    internal ClanWarLogUpdatedEventArgs(ClanWarLog? stored, ClanWarLog fetched, Clan? clan, CancellationToken cancellationToken) : base(cancellationToken)
    {
        Fetched = fetched;
        Clan = clan;
        Stored = stored;
    }
}
