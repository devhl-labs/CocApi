using devhl.CocApi.Models.Clan;
using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi
{
    public class ChangedEventArgs<T> : EventArgs
    {
#nullable disable

        public T Fetched { get; }

        public ChangedEventArgs(T fetched)
        {
            Fetched = fetched;
        }
    }



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

    public class ChangedEventArgs<T1, T2, T3> : EventArgs
    {
#nullable disable

        public T1 Fetched { get; }

        public T2 FetchedValue { get; }

        public T3 StoredValue { get; }

        public ChangedEventArgs(T1 fetched, T2 fetchedValue, T3 storedValue)
        {
            Fetched = fetched;

            FetchedValue = fetchedValue;

            StoredValue = storedValue;
        }
    }
}
