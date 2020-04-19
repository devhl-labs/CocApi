using Newtonsoft.Json;

namespace devhl.CocApi.Models
{
    public class WarLeague
    {
        [JsonProperty]
        public int Id { get; private set; }

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;        
    }
}
