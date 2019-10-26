using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public interface IVillageAPIModel
    {
        string VillageTag { get; set; }

        string Name { get; set; }

        string ClanTag { get; set; }
    }
}
