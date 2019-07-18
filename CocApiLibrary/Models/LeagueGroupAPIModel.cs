using System.Collections.Generic;
using System.Text.Json.Serialization;
using static CocApiStandardLibrary.Enums;

namespace CocApiStandardLibrary.Models
{
    public class LeagueGroupAPIModel
    {
        public State State { get; set; }

        public string Season { get; set; } = string.Empty;

        public IEnumerable<LeagueClanAPIModel>? Clans { get; set; }

        public IEnumerable<RoundAPIModel>? Rounds { get; set; }
    }
}
