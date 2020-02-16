using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Newtonsoft.Json;

using devhl.CocApi.Converters;
//using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.War
{
    public class LeagueGroup : Downloadable, ILeagueGroup, IInitialize
    {
        [JsonProperty]
        public LeagueState State { get; }

        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime Season { get; private set; }


        [JsonProperty]
        public IEnumerable<LeagueClan>? Clans { get; internal set; }

        [JsonProperty]
        public IList<Round>? Rounds { get; private set; }



        [JsonProperty]
        public int TeamSize { get; internal set; } = 15;

        /// <summary>
        /// This is the season and the first clan tag where the clans are sorted alphabetically.
        /// </summary>

        [JsonProperty]
        public string GroupKey { get; internal set; } = string.Empty;


        public void Initialize()
        {
            GroupKey = $"{Season.ToString()};{Clans.OrderBy(c => c.ClanTag).First().ClanTag}";

            foreach (var leagueClan in Clans.EmptyIfNull())
            {
                leagueClan.GroupId = GroupKey;

                leagueClan.LeagueClanKey = $"{Season.ToShortDateString()};{leagueClan.ClanTag}";

                foreach (var leagueVillage in leagueClan.Villages.EmptyIfNull())
                {
                    leagueVillage.ClanTag = leagueClan.ClanTag;

                    leagueVillage.LeagueClanKey = leagueClan.LeagueClanKey;
                }
            }

            foreach (var round in Rounds.EmptyIfNull())
            {
                round.RoundKey = $"{Season.ToShortDateString()};{Clans.OrderBy(c => c.ClanTag).First().ClanTag};{Rounds!.IndexOf(round)}";

                round.GroupKey = GroupKey;
            }
        }

        public override string ToString() => Season.ToString();
    }
}
