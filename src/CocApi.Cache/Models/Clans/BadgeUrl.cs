using Newtonsoft.Json;
using System.Linq;

namespace CocApi.Cache.Models.Clans
{
    public class BadgeUrl
    {
        [JsonProperty]
        public string Small { get; internal set; } = string.Empty;
        
        [JsonProperty]
        public string Large { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Medium { get; internal set; } = string.Empty;
    }
}
