using Newtonsoft.Json;

namespace CocApi.Cache.Models.Villages
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
