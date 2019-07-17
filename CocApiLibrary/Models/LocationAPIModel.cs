using System.Text.Json.Serialization;

namespace CocApiStandardLibrary.Models
{
    public class LocationModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsCountry { get; set; }

        public string CountryCode { get; set; } = string.Empty;
    }
}
