using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CocApiStandardLibrary.Models
{
    public class LeagueGroupAPIModel
    {
        public string State { get; set; } = string.Empty;

        public string Season { get; set; } = string.Empty;

        public IEnumerable<LeagueClanAPIModel>? Clans { get; set; }

        public IEnumerable<RoundAPIModel>? Rounds { get; set; }
    }
}
