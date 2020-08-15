using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Cache
{
    public class ClanQueryOptions
    {
        public string? ClanName { get; set; }
        public WarFrequency? WarFrequency { get; set; }
        public int? LocationId { get; set; }
        public int? MinVillages { get; set; }
        public int? MaxVillages { get; set; }
        public int? MinClanPoints { get; set; }
        public int? MinClanLevel { get; set; }
        public int? Limit { get; set; }
        public int? After { get; set; }
        public int? Before { get; set; }
    }
}
