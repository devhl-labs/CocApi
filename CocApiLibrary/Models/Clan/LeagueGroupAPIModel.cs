using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class LeagueGroupAPIModel : IDownloadable
    {
        public LeagueState State { get; set; }

        public string Season { get; set; } = string.Empty;

        public IEnumerable<LeagueClanAPIModel>? Clans { get; set; }

        public IEnumerable<RoundAPIModel>? Rounds { get; set; }

        public DateTime DateTimeUTC { get; internal set; } = DateTime.UtcNow;

        public DateTime Expires { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;

        public bool IsExpired()
        {
            if (DateTime.UtcNow > Expires)
            {
                return true;
            }
            return false;
        }

        //public void SetExpiration()
        //{
        //    DateTimeUTC = DateTime.UtcNow;

        //    Expires = DateTime.UtcNow.AddMinutes(15);
        //}
    }
}
