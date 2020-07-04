using Newtonsoft.Json;
using System.Collections.Generic;

namespace devhl.CocApi.Models.War
{
    public class Round
    {
        [JsonProperty]
        public List<string>? WarTags { get; private set; }
    }
}
