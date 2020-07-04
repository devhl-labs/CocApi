using Newtonsoft.Json;

namespace devhl.CocApi.Models.Clan
{
    public class Location
    {
        public static string Url() => $"locations?limit=10000";

        [JsonProperty]
        public int Id { get; internal set; }

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty]
        public bool IsCountry { get; internal set; }

        /// <summary>
        /// This only appears when looking up the clantag
        /// It does not appear when looking up locations.
        /// </summary>
        [JsonProperty]
        public string? CountryCode { get; internal set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name;
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
