using devhl.CocApi.Models.Clan;
using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi
{
    public class ModelEventArgs<T> : EventArgs
    {
#nullable disable

        public T Fetched { get; }

        public ModelEventArgs(T fetched)
        {
            Fetched = fetched;
        }
    }
}
