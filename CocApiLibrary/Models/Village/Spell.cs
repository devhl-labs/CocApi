using Newtonsoft.Json;

using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.Village
{
    public class Spell
    {
        [JsonProperty]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;

        [JsonProperty]
        public int Level { get; private set; }

        [JsonProperty]
        public int MaxLevel { get; private set; }

        [JsonProperty]
        public VillageType Village { get; internal set; }

        [JsonProperty]
        public int Order { get; internal set; }

        public override string ToString() => Name;
    }
}
