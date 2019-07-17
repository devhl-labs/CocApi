using System.Text.Json.Serialization;

namespace CocApiStandardLibrary.Models
{
    public class LeagueMemberAPIModel
    {
        public string Tag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int TownhallLevel { get; set; }

    }
}
