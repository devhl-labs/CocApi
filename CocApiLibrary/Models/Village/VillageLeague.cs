using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class VillageLeague : IInitialize
    {
        [JsonProperty]
        public int Id { get; }

        [JsonProperty]
        public string Name { get; } = string.Empty;

        [JsonProperty("IconUrls")]
        public VillageLeagueIconUrl? LeagueIconUrl { get; }

        [JsonProperty]
        public string? LeagueIconUrlId { get; private set; } = string.Empty;

        public void Initialize()
        {
            if (LeagueIconUrl != null)
            {
                LeagueIconUrl.Initialize();

                LeagueIconUrlId = LeagueIconUrl.Id;
            }
            else
            {
                LeagueIconUrlId = null;
            }


        }
    }
}
