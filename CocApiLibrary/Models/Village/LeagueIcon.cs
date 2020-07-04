using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class LeagueIcon
    {
        [JsonProperty]
        public string Medium { get; private set; } = string.Empty;

        [JsonProperty]
        public string Small { get; private set; } = string.Empty;

        [JsonProperty]
        public string Tiny { get; private set; } = string.Empty;
    }
}
