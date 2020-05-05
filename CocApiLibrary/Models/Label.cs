//using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace devhl.CocApi.Models
{
    public class Label : IInitialize
    {
        public static string ClanUrl() => $"https://api.clashofclans.com/v1/labels/clans?limit=10000";

        public static string VillageUrl() => $"https://api.clashofclans.com/v1/labels/players?limit=10000";

        [JsonProperty]
        public int Id { get; internal set; }

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;


        [JsonProperty("IconUrls")]
        public LabelUrl? LabelUrl { get; internal set; }


        public void Initialize(CocApi cocApi)
        {
            if (LabelUrl != null) LabelUrl.Id = Id;
        }

        public override string ToString() => Name;
    }
}