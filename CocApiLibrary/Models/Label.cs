using Newtonsoft.Json;

namespace devhl.CocApi.Models
{
    public class Label
    {
        public static string ClanUrl() => $"labels/clans?limit=10000";

        public static string VillageUrl() => $"labels/players?limit=10000";

        [JsonProperty]
        public int Id { get; internal set; }

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;


        [JsonProperty("IconUrls")]
        public LabelUrl? LabelUrl { get; internal set; }

        public override string ToString() => Name;
    }
}