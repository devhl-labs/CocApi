using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Newtonsoft.Json;

using devhl.CocApi.Converters;
using devhl.CocApi.Exceptions;
//using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.War
{
    public class LeagueGroup : Downloadable, ILeagueGroup, IInitialize
    {
        public static string Url(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar/leaguegroup";
        }

        public static string WarTag(string url)
        {
            url = url.Replace("clans/", "").Replace("/currentwar/leaguegroup", "");

            return Uri.UnescapeDataString(url);
        }

        [JsonProperty]
        public LeagueState State { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(LeagueSeasonConverter))]
        public DateTime Season { get; private set; }


        [JsonProperty]
        public IEnumerable<LeagueClan>? Clans { get; internal set; }

        [JsonProperty]
        public IList<Round>? Rounds { get; private set; }




        /// <summary>
        /// This is the season and the first clan tag where the clans are sorted alphabetically.
        /// </summary>

        public string GroupKey() => $"{Season:MM/yyyy};{Clans.OrderBy(c => c.ClanTag).First().ClanTag}";


        public void Initialize(CocApi cocApi)
        {
            foreach (LeagueClan leagueClan in Clans.EmptyIfNull())            
                foreach (var leagueVillage in leagueClan.Villages.EmptyIfNull())                
                    leagueVillage.ClanTag = leagueClan.ClanTag; 
        }

        public override string ToString() => Season.ToString();
    }
}
