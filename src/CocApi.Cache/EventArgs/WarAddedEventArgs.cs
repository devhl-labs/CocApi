using CocApi.Rest.Models;
using System.Threading;

namespace CocApi.Cache;

public class WarAddedEventArgs : CancellableEventArgs
{
    public Clan? Clan { get; }

    public Clan? Opponent { get; }

    public ClanWar War { get; }

    internal WarAddedEventArgs(Clan? clan, Clan? opponent, ClanWar war, CancellationToken cancellationToken) : base(cancellationToken)
    {
        Clan = clan;
        Opponent = opponent;
        War = war;
    }
}
