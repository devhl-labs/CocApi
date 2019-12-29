using Newtonsoft.Json;
using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.Village
{
    public class Troop
    {
        [JsonProperty]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; } = string.Empty;

        [JsonProperty]
        public int Level { get; }

        [JsonProperty]
        public int MaxLevel { get; }

        [JsonProperty]
        public VillageType Village { get; }

        [JsonProperty]
        public bool IsHero { get; internal set; }   
    }
}
