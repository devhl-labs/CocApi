using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class IconUrlsAPIModel
    {
        public string Medium { get; set; } = string.Empty;

        public string Small { get; set; } = string.Empty;

        public string Tiny { get; set; } = string.Empty;
    }
}
