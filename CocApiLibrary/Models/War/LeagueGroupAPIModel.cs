using CocApiLibrary.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class LeagueGroupApiModel : ILeagueGroup, IDownloadable
    {
        [JsonConverter(typeof(LeagueStateConverter))]
        public LeagueState State { get; set; }

        private DateTime _season;

        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime Season
        {
            get
            {
                return _season;
            }

            set
            {
                _season = value;

                SetRelationalProperties();
            }
        }

        [ForeignKey(nameof(GroupId))]
        public virtual IEnumerable<LeagueClanApiModel>? Clans { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual IList<RoundApiModel>? Rounds { get; set; }

        public DateTime UpdateAtUtc { get; internal set; } = DateTime.UtcNow;

        public DateTime Expires { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;

        public DateTime? CacheExpiresAtUtc { get; set; }

        [JsonIgnore]
        public int TeamSize { get; internal set; } = 15;



        private string _groupId = string.Empty;

        /// <summary>
        /// This is the season and the first clan tag where the clans are sorted alphabetically.
        /// </summary>
        [Key]
        public string GroupId
        {
            get
            {
                return _groupId;
            }

            set
            {
                _groupId = value;

                SetRelationalProperties();
            }
        }


        public bool IsExpired()
        {
            if (DateTime.UtcNow > Expires)
            {
                return true;
            }
            return false;
        }

        private void SetRelationalProperties()
        {
            if (_season != null && Clans?.Count() > 0)
            {
                _groupId = $"{_season.ToShortDateString()};{Clans.OrderBy(c => c.ClanTag).First().ClanTag}";
            }

            if (Clans?.Count() > 0 && _season != null)
            {
                foreach (var clan in Clans)
                {
                    clan.LeagueClanId = $"{_season.ToShortDateString()};{clan.ClanTag}";
                }
            }
        }
    }
}
