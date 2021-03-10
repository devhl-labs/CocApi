using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Cache
{
    public class CancellableEventArgs : EventArgs
    {
        public CancellationToken CancellationToken {get;}

        internal CancellableEventArgs(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }
    }
}
