using devhl.CocApi.Models.Village;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;
namespace devhl.CocApi.Models.War
{
    public class LeagueVillage : IVillage
    {
        [JsonProperty("Tag")]
        public string VillageTag { get; private set; } = string.Empty;


        [JsonProperty]
        public string Name { get; private set; } = string.Empty;


        [JsonProperty]
        public string ClanTag { get; internal set; } = string.Empty;


        [JsonProperty]
        public int TownhallLevel { get; private set; }

        [JsonProperty]
        public string LeagueClanId { get; internal set; } = string.Empty;

        public void Initialize()
        {

        }

        public override string ToString() => $"{VillageTag} {Name} {TownhallLevel}";
    }
}
