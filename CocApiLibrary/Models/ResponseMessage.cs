using Newtonsoft.Json;

namespace devhl.CocApi.Models
{
    public class ResponseMessage
    {
        [JsonProperty]
        public string Reason { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Message { get; internal set; } = string.Empty;
    }
}
