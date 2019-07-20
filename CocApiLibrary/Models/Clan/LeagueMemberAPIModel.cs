using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class LeagueMemberAPIModel : IVillageAPIModel
    {
        public string Tag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int TownhallLevel { get; set; }

    }
}
