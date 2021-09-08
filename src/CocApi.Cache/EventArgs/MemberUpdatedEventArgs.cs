using CocApi.Cache.Context.CachedItems;
using CocApi.Model;
using System;
using System.Threading;

namespace CocApi.Cache
{
    public class MemberUpdatedEventArgs : PlayerUpdatedEventArgs
    {
        public CachedClan Clan { get; }

        internal MemberUpdatedEventArgs(CachedClan clan, Player? stored, Player fetched, CancellationToken cancellationtoken) 
            : base(stored, fetched, cancellationtoken)
        {
            Clan = clan;
        }
    }
}
