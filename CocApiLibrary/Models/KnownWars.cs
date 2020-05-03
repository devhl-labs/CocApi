using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi.Models
{
    public class KnownWars
    {
        public DateTime PreparationStartTimeUtc { get; set; }

        public string ClanTag { get; set; } = string.Empty;
    }
}
