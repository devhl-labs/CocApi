using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

//using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.Village
{
    public class Achievement : IInitialize
    {
        [JsonProperty]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public int Stars { get; private set; }

        [JsonProperty]
        public int Value { get; private set; }

        [JsonProperty]
        public int Target { get; private set; }

        [JsonProperty]
        public string CompletionInfo { get; private set; } = string.Empty;

        [JsonProperty]
        public VillageType Village { get; private set; }

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;

        [JsonProperty]
        public string Info { get; private set; } = string.Empty;

        public void Initialize()
        {            
            if (Info == "Connect your account to Supercell ID for safe keeping.")
            {
                Name = "Keep your village safe2";
            }
        }

        public override string ToString() => Name;
    }
}
