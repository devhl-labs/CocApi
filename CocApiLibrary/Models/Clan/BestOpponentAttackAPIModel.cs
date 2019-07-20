using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class BestOpponentAttackAPIModel
    {
        public string AttackerTag { get; set; } = string.Empty;

        public string DefenderTag { get; set; } = string.Empty;

        public int Stars { get; set; }

        public int DestructionPercentage { get; set; }

        public int Order { get; set; }
    }
}
