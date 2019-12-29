using Newtonsoft.Json;
using System.Linq;

namespace devhl.CocApi.Models.Clan
{
    public class ClanBadgeUrl : IInitialize
    {
        public string Id { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Small { get; internal set; } = string.Empty;
        
        [JsonProperty]
        public string Large { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Medium { get; internal set; } = string.Empty;

        public void Initialize()
        {
            if (!string.IsNullOrEmpty(Small)) Id = Small.Split("/").Last();

            if (!string.IsNullOrEmpty(Medium)) Id = Small.Split("/").Last();

            if (!string.IsNullOrEmpty(Large)) Id = Small.Split("/").Last();
        }
    }
}
