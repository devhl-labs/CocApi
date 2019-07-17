using System.Text.Json.Serialization;

namespace CocApiStandardLibrary.Models
{
    public interface IVillageAPIModel
    {
        string Tag { get; }
        string ClanTag { get; }
    }
}
