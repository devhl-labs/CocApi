using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class LocationModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsCountry { get; set; }

        /// <summary>
        /// This only appears when looking up the clantag
        /// It does not appear when looking up locations.
        /// </summary>
        public string CountryCode { get; set; } = string.Empty;
    }
}
