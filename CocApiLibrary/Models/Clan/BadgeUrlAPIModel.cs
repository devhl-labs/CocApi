using System;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class BadgeUrlModel
    {
        public string Tag { get; set; } = string.Empty;

        public string Small { get; set; } = string.Empty;

        public string Large { get; set; } = string.Empty;

        public string Medium { get; set; } = string.Empty;
    }
}
