using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace devhl.CocApi.Models
{
    public class Label : IInitialize
    {
        [JsonProperty]
        public int Id { get; }

        [JsonProperty]
        public string Name { get; private set; } = string.Empty;


        [JsonProperty("IconUrls")]
        public LabelUrl? LabelUrl { get; internal set; }


        public void Initialize()
        {
            if (LabelUrl != null) LabelUrl.Id = Id;
        }
    }
}