using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.Village
{
    public class Achievement : IInitialize
    {
        [JsonProperty]
        public string VillageTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public int Stars { get; }

        [JsonProperty]
        public int Value { get; }

        [JsonProperty]
        public int Target { get; }

        [JsonProperty]
        public string CompletionInfo { get; } = string.Empty;

        [JsonProperty]
        public VillageType Village { get; }

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;

        [JsonProperty]
        public string Info { get; } = string.Empty;

        public void Initialize()
        {            
            if (Info == "Connect your account to Supercell ID for safe keeping.")
            {
                Name = "Keep your village safe2";
            }
        }
    }
}
