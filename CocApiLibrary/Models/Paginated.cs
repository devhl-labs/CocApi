using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace devhl.CocApi.Models
{
    public class Paginated<T> : Downloadable, IInitialize
    {
        [JsonProperty]
        public IEnumerable<T>? Items { get; private set; }

        [JsonProperty]
        public Paging? Paging { get; private set; }

        public void Initialize(CocApi cocApi)
        {
            foreach(var item in Items.EmptyIfNull())
            {
                if (item is IInitialize initialize)
                {
                    initialize.Initialize(cocApi);
                }
            }
        }
    }
}
