

using devhl.CocApi.Exceptions;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace devhl.CocApi.Models.War
{
    public class LeagueWar : CurrentWar, IInitialize, IWar
    {
        public static new string Url(string warTag)
        {
            if (CocApi.TryGetValidTag(warTag, out string formattedTag) == false)
                throw new InvalidTagException(warTag);

            return $"clanwarleagues/wars/{Uri.EscapeDataString(formattedTag)}";
        }

        public static string WarTagFromUrl(string url)
        {
            url = url.Replace("clanwarleagues/wars/", "");

            return Uri.UnescapeDataString(url);
        }

        [JsonProperty]
        public string WarTag { get; internal set; } = string.Empty;

        /// <summary>
        /// Library generated value which is a foreign key for the league group.
        /// </summary>
        [JsonProperty]
        public string GroupKey { get; internal set; } = string.Empty;

        public new void Initialize(CocApi cocApi)
        {
            base.Initialize(cocApi);

            WarType = WarType.SCCWL;
        }
    }
}
