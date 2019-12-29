using Newtonsoft.Json;


namespace devhl.CocApi.Models.Village
{
    public class VillageLabel : Label
    {
        [JsonProperty]
        public string VillageTag { get; internal set; } = string.Empty;
    }
}
