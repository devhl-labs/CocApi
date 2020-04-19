using Newtonsoft.Json;
//using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace devhl.CocApi.Models.Village
{
    public class LeagueIcon
    {
        [JsonProperty]
        public int LeaugeId { get; internal set; }

        [JsonProperty]
        public string Medium { get; private set; } = string.Empty;

        [JsonProperty]
        public string Small { get; private set; } = string.Empty;

        [JsonProperty]
        public string Tiny { get; private set; } = string.Empty;
    }
}
