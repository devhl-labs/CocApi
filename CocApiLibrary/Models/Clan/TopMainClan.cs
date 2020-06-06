using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.Clan
{
    public class TopMainClan : TopClan, IClan
    {
        public static string Url(int? locationId)
        {
            string location = "global";

            if (locationId != null)
                location = locationId.ToString();

            return $"{location}/rankings/clans";
        }

        [JsonProperty("clanPoints")]
        public int ClanPoints { get; internal set; }

    }
}
