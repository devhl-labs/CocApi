using CocApi.Cache.Models.Wars;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CocApi.Cache.Models
{
    public class Paginated<T> : Downloadable, IInitialize
    {
        [JsonProperty]
        public IEnumerable<T>? Items { get; private set; }

        [JsonProperty]
        public Paging? Paging { get; private set; }

        public void Initialize(CocApiClient_old cocApi)
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

    public class WarLog : Paginated<WarLogEntry>, IWar
    {
    }
}
