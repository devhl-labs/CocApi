using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
//System.Text.Json.Serialization
using Newtonsoft.Json;

using devhl.CocApi.Converters;
using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.War
{
    public class LeagueGroupApiModel : Downloadable, ILeagueGroup, IInitialize /*, IDownloadable*/
    {
        //[JsonConverter(typeof(LeagueStateConverter))]
        
        public LeagueState State { get; set; }

        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime Season { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual IEnumerable<LeagueClanApiModel>? Clans { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual IList<RoundApiModel>? Rounds { get; set; }

        //public DateTime UpdatedAtUtc { get; set; }

        //public DateTime ExpiresAtUtc { get; set; }

        //public string EncodedUrl { get; set; } = string.Empty;

        //public DateTime? CacheExpiresAtUtc { get; set; }

        //[JsonIgnore]
        public int TeamSize { get; set; } = 15;

        /// <summary>
        /// This is the season and the first clan tag where the clans are sorted alphabetically.
        /// </summary>
        [Key]
        public string GroupId { get; set; } = string.Empty;


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
            GroupId = $"{Season.ToString()};{Clans.OrderBy(c => c.ClanTag).First().ClanTag}";

            foreach (var leagueClan in Clans.EmptyIfNull())
            {
                leagueClan.GroupId = GroupId;

                leagueClan.LeagueClanId = $"{Season.ToShortDateString()};{leagueClan.ClanTag}";

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
