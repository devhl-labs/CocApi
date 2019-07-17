using System.Text.Json.Serialization;

namespace CocApiStandardLibrary.Models
{
    public class TroopAPIModel
    {
        public string Name { get; set; } = string.Empty;

        public int Level { get; set; }

        public int MaxLevel { get; set; }

        public string Village { get; set; } = string.Empty;
    }
}
