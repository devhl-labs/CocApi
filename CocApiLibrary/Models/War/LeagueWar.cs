

using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class LeagueWar : CurrentWar, IInitialize
    {
        [JsonProperty]
        public string WarTag { get; internal set; } = string.Empty;

        public new void Initialize()
        {
            base.Initialize();

            WarType = WarType.SCCWL;
        }
    }
}
