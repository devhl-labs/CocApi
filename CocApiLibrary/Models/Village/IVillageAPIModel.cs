using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public interface IVillageAPIModel
    {
        string Tag { get; }
        string Name { get; }
    }
}
