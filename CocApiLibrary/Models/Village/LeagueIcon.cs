using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace devhl.CocApi.Models.Village
{
    public class LeagueIcon
    {
        [JsonProperty]
        public int LeaugeId { get; internal set; }

        [JsonProperty]
        public string Medium { get; } = string.Empty;

        [JsonProperty]
        public string Small { get; } = string.Empty;

        [JsonProperty]
        public string Tiny { get; } = string.Empty;
    }
}
