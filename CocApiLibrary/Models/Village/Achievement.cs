using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class Achievement : IInitialize
    {
        [JsonProperty]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public int Stars { get; internal set; }

        [JsonProperty]
        public int Value { get; internal set; }

        [JsonProperty]
        public int Target { get; internal set; }

        [JsonProperty]
        public string CompletionInfo { get; internal set; } = string.Empty;

        [JsonProperty]
        public VillageType Village { get; internal set; }

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Info { get; internal set; } = string.Empty;

        public void Initialize(CocApi cocApi)
        {            
            if (Info == "Connect your account to Supercell ID for safe keeping.")
            {
                Name = "Keep your village safe2";
            }
        }

        public override string ToString() => Name;
    }
}
