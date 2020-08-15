using Newtonsoft.Json;

namespace CocApi.Cache.Models.Clans
{
    public class TopBuilderClan : TopClan, IClan
    {
        public static string Url(int? locationId)
        {
            string location = "global";

            if (locationId != null)
                location = locationId.ToString();

            return $"{location}/rankings/clans-versus";
        }

        [JsonProperty("clanVersusPoints")]
        public int ClanVersusPoints { get; internal set; }
    }
}
