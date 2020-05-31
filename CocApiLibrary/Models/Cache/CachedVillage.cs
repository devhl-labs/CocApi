using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi.Models.Cache
{
    public class CachedVillage
    {
        public string VillageTag { get; set; } = string.Empty;

        public string? Json { get; set; }

        public bool Download { get; set; }

        public DateTime UpdatesAt { get; set; }
    }
}
