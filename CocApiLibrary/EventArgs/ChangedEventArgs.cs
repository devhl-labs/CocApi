using devhl.CocApi.Models.Clan;
using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi
{
    public class ChangedEventArgs<T1, T2> : EventArgs
    {
#nullable disable

        public T1 Fetched { get; }

        public T2 Value { get; }

        public ChangedEventArgs(T1 fetched, T2 value)
        {
            Fetched = fetched;

            Value = value;
        }
    }
}
