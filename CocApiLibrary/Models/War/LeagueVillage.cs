using devhl.CocApi.Models.Village;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;
namespace devhl.CocApi.Models.War
{
    public class LeagueVillage : IVillage
    {
        [JsonProperty("Tag")]
        public string VillageTag { get; } = string.Empty;


        [JsonProperty]
        public string Name { get; } = string.Empty;


        [JsonProperty]
        public string ClanTag { get; internal set; } = string.Empty;


        [JsonProperty]
        public int TownhallLevel { get; }

        [JsonProperty]
        public string LeagueClanId { get; internal set; } = string.Empty;

        public void Initialize()
        {

        }
    }
}
