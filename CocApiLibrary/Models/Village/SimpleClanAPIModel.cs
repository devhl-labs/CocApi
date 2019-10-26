using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class SimpleClanAPIModel : IClanAPIModel
    {
        // IClanAPIModel
        [Key]
        [JsonPropertyName("Tag")]
        public string ClanTag { get; set; } = string.Empty;

        public int ClanLevel { get; set; }

        public string Name { get; set; } = string.Empty;

        public BadgeUrlModel? BadgeUrls { get; set; }
    }
}
