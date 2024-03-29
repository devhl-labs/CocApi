using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CocApi.Rest.Models
{
    public partial class ClanWarAttack
    {
        [JsonPropertyName("attackerClanTag")]
        public string AttackerClanTag { get; internal set; }


        [JsonPropertyName("defenderClanTag")]
        public string DefenderClanTag { get; internal set; }


        [JsonPropertyName("starsGained")]
        public int StarsGained { get; internal set; }


        [JsonPropertyName("fresh")]
        public bool Fresh { get; internal set; } = false;


        [JsonPropertyName("attackerTownHall")]
        public int AttackerTownHall { get; internal set; }


        [JsonPropertyName("defenderTownHall")]
        public int DefenderTownHall { get; internal set; }


        [JsonPropertyName("attackerMapPosition")]
        public int AttackerMapPosition { get; internal set; }


        [JsonPropertyName("defenderMapPosition")]
        public int DefenderMapPosition { get; internal set; }
    }
}
