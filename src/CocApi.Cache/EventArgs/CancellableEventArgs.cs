using System;
using System.Threading;

namespace CocApi.Cache;

public class CancellableEventArgs : EventArgs
{
    public CancellationToken CancellationToken {get;}

    internal CancellableEventArgs(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
    }
}
