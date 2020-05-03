using Newtonsoft.Json;

namespace devhl.CocApi.Models
{
    public class WarLeague
    {
        public static string Url() => $"https://api.clashofclans.com/v1/warleagues?limit=10000";

        [JsonProperty]
        public int Id { get; private set; }

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;        
    }
}
