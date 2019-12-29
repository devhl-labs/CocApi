using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class League : IInitialize
    {
        [JsonProperty]
        public int Id { get; }

        [JsonProperty]
        public string Name { get; } = string.Empty;

        [JsonProperty("IconUrls")]
        public LeagueIcon? LeagueIcon { get; internal set; }

        public void Initialize()
        {
            if (LeagueIcon != null) LeagueIcon.LeaugeId = Id;
        }
    }
}
