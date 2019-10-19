using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class TroopAPIModel
    {
        public string Tag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Level { get; set; }

        public int MaxLevel { get; set; }

        public Village Village { get; set; }
    }
}
