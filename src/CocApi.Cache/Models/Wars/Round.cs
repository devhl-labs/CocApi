using Newtonsoft.Json;
using System.Collections.Generic;

namespace CocApi.Cache.Models.Wars
{
    public class Round
    {
        [JsonProperty]
        public List<string>? WarTags { get; private set; }
    }
}
