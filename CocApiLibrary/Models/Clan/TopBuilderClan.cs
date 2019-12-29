using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.Clan
{
    public class TopBuilderClan : TopClan, IClan
    {
        [JsonProperty("clanVersusPoints")]
        public int ClanVersusPoints { get; internal set; }
    }
}
