using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class SimpleClanAPIModel : IClanAPIModel
    {
        public string Tag { get; set; } = string.Empty;

        public int ClanLevel { get; set; }

        public string Name { get; set; } = string.Empty;

        public BadgeUrlModel? BadgeUrls { get; set; }
    }
}
