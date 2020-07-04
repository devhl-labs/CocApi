using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class Troop
    {
        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty]
        public int Level { get; internal set; }

        [JsonProperty]
        public int MaxLevel { get; internal set; }

        [JsonProperty]
        public VillageType Village { get; internal set; }

        [JsonProperty]
        public bool IsHero { get; internal set; }   

        [JsonProperty]
        public int Order { get; internal set; }

        public override string ToString() => Name;
    }
}
