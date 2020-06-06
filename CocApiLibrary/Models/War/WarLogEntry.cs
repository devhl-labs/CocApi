using devhl.CocApi.Converters;
using System;
using Newtonsoft.Json;
using devhl.CocApi.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace devhl.CocApi.Models.War
{
    public class WarLogEntry : Downloadable, IInitialize, IWar
    {
        public WarLogEntry()
        {
            //ServerExpirationUtc = DateTime.MaxValue;

            //LocalExpirationUtc = DateTime.MaxValue;

            DownloadedAtUtc = DateTime.UtcNow;
        }

        public static string Url(string clanTag, int? limit = null, int? after = null, int? before = null)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            string url = $"clans/";

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
        [JsonConverter(typeof(ResultConverter))]
        private Result Result { get; set; }

        [JsonProperty]
        public int TeamSize { get; private set; }

        [JsonProperty]
        public WarLogEntryClan? Clan { get; private set; }

        [JsonProperty]
        public WarLogEntryClan? Opponent { get; private set; }

        [JsonProperty]
        public List<WarLogEntryClan> WarClans { get; private set; } = new List<WarLogEntryClan>();

        [JsonProperty("endTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EndTimeUtc { get; private set; }

        public override string ToString() => EndTimeUtc.ToString();

        public void Initialize(CocApi cocApi)
        {
            if (Clan != null)
            {
                WarClans.Add(Clan);

                if (Result == Result.Lose)
                    Clan.Result = Result.Lose;

                if (Result == Result.Win)
                    Clan.Result = Result.Win;

                if (Result == Result.Tie)
                    Clan.Result = Result.Tie;

                if (Result == Result.Null)
                    Clan.Result = Result.Null;
            }


            if (Opponent != null)
            {
                WarClans.Add(Opponent);

                if (Result == Result.Lose)
                    Opponent.Result = Result.Lose;

                if (Result == Result.Win)
                    Opponent.Result = Result.Win;

                if (Result == Result.Tie)
                    Opponent.Result = Result.Tie;

                if (Result == Result.Null)
                    Opponent.Result = Result.Null;
            }

            if (WarClans.All(wc => wc.ClanTag != null))
                WarClans = WarClans.OrderBy(wc => wc.ClanTag).ToList();
        }
    }
}