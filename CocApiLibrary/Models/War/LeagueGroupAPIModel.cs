using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

using devhl.CocApi.Converters;
using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models
{
    public class LeagueGroupApiModel : Downloadable, ILeagueGroup, IInitialize /*, IDownloadable*/
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

                //SetRelationalProperties();
            }
        }

        [ForeignKey(nameof(GroupId))]
        public virtual IEnumerable<LeagueClanApiModel>? Clans { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual IList<RoundApiModel>? Rounds { get; set; }

        //public DateTime UpdatedAtUtc { get; set; }

        //public DateTime ExpiresAtUtc { get; set; }

        //public string EncodedUrl { get; set; } = string.Empty;

        //public DateTime? CacheExpiresAtUtc { get; set; }

        [JsonIgnore]
        public int TeamSize { get; set; } = 15;



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

                //SetRelationalProperties();
            }
        }


        //public bool IsExpired()
        //{
        //    if (DateTime.UtcNow > ExpiresAtUtc)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        public void Initialize()
        {
            //if (_season != null && Clans?.Count() > 0)
            //{
            //    _groupId = $"{_season.ToShortDateString()};{Clans.OrderBy(c => c.ClanTag).First().ClanTag}";
            //}

            //if (Clans?.Count() > 0 && _season != null)
            //{
            //    foreach (var clan in Clans)
            //    {
            //        clan.LeagueClanId = $"{_season.ToShortDateString()};{clan.ClanTag}";
            //    }
            //}

            GroupId = $"{Season.ToString()};{Clans.OrderBy(c => c.ClanTag).First().ClanTag}";

            //foreach (var leagueClan in Clans.EmptyIfNull())
            //{
            //    leagueClan.GroupId = GroupId;

            //    foreach (var leagueVillage in leagueClan.Villages.EmptyIfNull())
            //    {
            //        leagueVillage.ClanTag = leagueClan.ClanTag;
            //    }
            //}

            //foreach (var round in Rounds.EmptyIfNull())
            //{
            //    round.RoundId = $"{Season.ToShortDateString()};{Clans.OrderBy(c => c.ClanTag).First().ClanTag};{Rounds!.IndexOf(round)}";
            //}

            //GroupId = $"{Season.ToString()}{Clans.OrderBy(c => c.ClanTag).First().ClanTag}";

            foreach (var leagueClan in Clans.EmptyIfNull())
            {
                leagueClan.GroupId = GroupId;

                leagueClan.LeagueClanId = $"{_season.ToShortDateString()};{leagueClan.ClanTag}";

                foreach (var leagueVillage in leagueClan.Villages.EmptyIfNull())
                {
                    leagueVillage.ClanTag = leagueClan.ClanTag;

                    leagueVillage.LeagueClanId = leagueClan.LeagueClanId;
                }
            }

            foreach (var round in Rounds.EmptyIfNull())
            {
                round.RoundId = $"{Season.ToShortDateString()};{Clans.OrderBy(c => c.ClanTag).First().ClanTag};{Rounds!.IndexOf(round)}";

                round.GroupId = GroupId;
            }
        }
    }
}
