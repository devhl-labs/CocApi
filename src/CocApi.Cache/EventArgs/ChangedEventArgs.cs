using CocApi.Cache.Models.Clans;
using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Cache
{
    public class EventArgs<T> : EventArgs
    {
        public T Fetched { get; }

        public EventArgs(T fetched)
        {
            Fetched = fetched;
        }
    }



    public class ChangedEventArgs<T> : EventArgs
    {
        public T Fetched { get; }

        public T Value { get; }

        public ChangedEventArgs(T fetched, T value)
        {
            Fetched = fetched;

            Value = value;
        }
    }

    public class ChangedEventArgs<T1, T2> : EventArgs
    {
        public T1 Fetched { get; }

        public T2 Value { get; }

        public ChangedEventArgs(T1 fetched, T2 value)
        {
            Fetched = fetched;

            Value = value;
        }
    }

    public class ChildChangedEventArgs<T1, T2> : EventArgs
    {
        public T1 Fetched { get; }

        public T2 FetchedValue { get; }

        public T2 StoredValue { get; }

        public ChildChangedEventArgs(T1 fetched, T2 fetchedValue, T2 storedValue)
        {
            Fetched = fetched;

            FetchedValue = fetchedValue;

            StoredValue = storedValue;
        }
    }
}
