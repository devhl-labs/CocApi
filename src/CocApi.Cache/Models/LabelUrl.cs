using Newtonsoft.Json;

namespace CocApi.Cache.Models
{
    public class LabelUrl
    {
        [JsonProperty]
        public string Medium { get; private set; } = string.Empty;

        [JsonProperty]
        public string Small { get; private set; } = string.Empty;
        
    }
}
