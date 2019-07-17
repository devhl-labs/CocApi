using System.Text.Json.Serialization;

namespace CocApiStandardLibrary.Models
{
    public class AchievementAPIModel
    {
        public string Name { get; set; } = string.Empty;

        public int Stars { get; set; }

        public int Value { get; set; }

        public int Target { get; set; }

        public string Info { get; set; } = string.Empty;

        public string CompletionInfo { get; set; } = string.Empty;

        public string Village { get; set; } = string.Empty;
    }
}
