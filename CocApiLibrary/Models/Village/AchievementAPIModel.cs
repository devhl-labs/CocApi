using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class AchievementAPIModel
    {
        public string Name { get; set; } = string.Empty;

        public int Stars { get; set; }

        public int Value { get; set; }

        public int Target { get; set; }

        public string Info { get; set; } = string.Empty;

        public string CompletionInfo { get; set; } = string.Empty;

        public Village Village { get; set; }
    }
}
