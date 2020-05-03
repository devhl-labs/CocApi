using devhl.CocApi.Converters;
using System;

//using static devhl.CocApi.Enums;
using Newtonsoft.Json;
using devhl.CocApi.Exceptions;

namespace devhl.CocApi.Models.War
{
    public class WarLogEntry
    {
        public static string Url(string clanTag, int? limit = null, int? after = null, int? before = null)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            string url = $"https://api.clashofclans.com/v1/clans/";

            url = $"{url}{Uri.EscapeDataString(formattedTag)}/warlog?";

            if (limit != null)
                url = $"{url}limit={limit}&";

            if (after != null)
                url = $"{url}after={after}&";

            if (before != null)
                url = $"{url}before={before}&";

            if (url.EndsWith("&"))
                url = url[0..^1];

            if (url.EndsWith("?"))
                url = url[0..^1];

            return url;
        }

        [JsonProperty]
        public Result Result { get; private set; }

        [JsonProperty]
        public int TeamSize { get; private set; }

        [JsonProperty]
        public WarClan? Clan { get; private set; }

        [JsonProperty]
        public WarClan? Opponent { get; private set; }

        [JsonProperty("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUtc { get; private set; }

        public override string ToString() => EndTimeUtc.ToString();
    }
}