using devhl.CocApi.Models.Clan;
using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi
{
    public class LabelsChangedEventArgs<T1, T2> : EventArgs
    {
#nullable disable

        public T1 Fetched { get; }

        public IReadOnlyList<T2> Added { get; }
        
        public IReadOnlyList<T2> Removed { get; }

        public LabelsChangedEventArgs(T1 fetched, IReadOnlyList<T2> added, IReadOnlyList<T2> removed)
        {
            Fetched = fetched;

            Added = added;

            Removed = removed;
        }
    }
}
