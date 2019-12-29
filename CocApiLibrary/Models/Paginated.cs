using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace devhl.CocApi.Models
{
    public class Paginated<T> : Downloadable, IInitialize
    {
        [JsonProperty]
        public IEnumerable<T>? Items { get; }

        [JsonProperty]
        public Paging? Paging { get; }

        public void Initialize()
        {
            foreach(var item in Items.EmptyIfNull())
            {
                if (item is IInitialize initialize)
                {
                    initialize.Initialize();
                }
            }
        }
    }
}
