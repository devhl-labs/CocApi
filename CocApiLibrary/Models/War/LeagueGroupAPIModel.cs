using CocApiLibrary.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class LeagueGroupAPIModel : IDownloadable
    {
        [JsonConverter(typeof(LeagueStateConverter))]
        public LeagueState State { get; set; }

        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime Season { get; set; }

        public IEnumerable<LeagueClanAPIModel>? Clans { get; set; }

        public IList<RoundAPIModel>? Rounds { get; set; }

        public DateTime DateTimeUTC { get; internal set; } = DateTime.UtcNow;

        public DateTime Expires { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;

        [JsonIgnore]
        public int TeamSize { get; internal set; } = 15;


        /// <summary>
        /// This is the season and the first clan tag where the clans are sorted alphabetically.
        /// </summary>
        public string GroupID { get; internal set; } = string.Empty;


        public bool IsExpired()
        {
            if (DateTime.UtcNow > Expires)
            {
                return true;
            }
            return false;
        }
    }
}
