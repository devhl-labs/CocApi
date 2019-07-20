using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class SpellModel
    {
        public string Name { get; set; } = string.Empty;

        public int Level { get; set; }

        public int MaxLevel { get; set; }

        public string Village { get; set; } = string.Empty;
    }
}
