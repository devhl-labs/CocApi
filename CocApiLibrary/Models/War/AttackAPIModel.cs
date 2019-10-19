using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class AttackAPIModel
    {
        public string WarID { get; internal set; } = string.Empty;

        public string AttackerTag { get; set; } = string.Empty;

        public string DefenderTag { get; set; } = string.Empty;

        public int Stars { get; set; }

        public int DestructionPercentage { get; set; }

        public int Order { get; set; }





        [JsonIgnore]
        public bool Fresh { get; set; } = false;

        [JsonIgnore]
        public string AttackerClanTag { get; set; } = string.Empty;

        [JsonIgnore]
        public string DefenderClanTag { get; set; } = string.Empty;
    }
}
