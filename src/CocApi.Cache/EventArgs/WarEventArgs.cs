using CocApi.Rest.Models;
using System.Threading;

namespace CocApi.Cache
{
    public class WarEventArgs : CancellableEventArgs
    {
        public ClanWar War { get; }

        internal WarEventArgs(ClanWar war, CancellationToken cancellationToken) : base(cancellationToken)
        {
            War = war;
        }
    }
}
