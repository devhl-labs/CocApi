using devhl.CocApi.Models;
using devhl.CocApi.Models.Clan;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace devhl.CocApi
{
    public class LabelsChangedEventArgs<T> : EventArgs
    {
        public T Fetched { get; }

        public ImmutableArray<Label> Added { get; }
        
        public ImmutableArray<Label> Removed { get; }

        public LabelsChangedEventArgs(T fetched, ImmutableArray<Label> added, ImmutableArray<Label> removed)
        {
            Fetched = fetched;

            Added = added;

            Removed = removed;
        }
    }
}
