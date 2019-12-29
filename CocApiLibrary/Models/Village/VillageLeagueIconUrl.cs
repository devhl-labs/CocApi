using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace devhl.CocApi.Models.Village
{
    public class VillageLeagueIconUrl : IInitialize
    {
        [JsonProperty]
        public string Id { get; private set; } = string.Empty;

        [JsonProperty]
        public string Medium { get; } = string.Empty;

        [JsonProperty]
        public string Small { get; } = string.Empty;

        [JsonProperty]
        public string Tiny { get; } = string.Empty;
        
        public void Initialize()
        {
            if (!string.IsNullOrEmpty(Medium)) Id = Medium.Split("/").Last();

            if (!string.IsNullOrEmpty(Small)) Id = Small.Split("/").Last();

            if (!string.IsNullOrEmpty(Tiny)) Id = Tiny.Split("/").Last();
        }
    }
}
