using Newtonsoft.Json;

namespace CocApi.Cache.Models
{
    public class WarLeague
    {
        public static string Url() => $"warleagues?limit=10000";

        [JsonProperty]
        public int Id { get; private set; }

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;        
    }
}
