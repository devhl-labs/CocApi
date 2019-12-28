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
    public class LeagueGroupApiModel : Downloadable, ILeagueGroup, IInitialize
    {

        
        public LeagueState State { get; set; }

        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime Season { get; set; }


        public IEnumerable<LeagueClanApiModel>? Clans { get; set; }

        public IList<RoundApiModel>? Rounds { get; set; }



        public int TeamSize { get; set; } = 15;

        /// <summary>
        /// This is the season and the first clan tag where the clans are sorted alphabetically.
        /// </summary>

        public string GroupId { get; set; } = string.Empty;


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
