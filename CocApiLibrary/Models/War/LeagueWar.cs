

using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class LeagueWar : CurrentWar, IInitialize
    {
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

            ILeagueGroup? iLeagueGroup = cocApi.GetLeagueGroupOrDefault(WarClans[0].ClanTag);

            iLeagueGroup ??= cocApi.GetLeagueGroupOrDefault(WarClans[1].ClanTag);

            if (iLeagueGroup is LeagueGroup leagueGroup)
            {
                GroupKey = leagueGroup.GroupKey;
            }

        }
    }
}
