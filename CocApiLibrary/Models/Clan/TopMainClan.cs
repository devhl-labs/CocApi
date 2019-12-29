using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace devhl.CocApi.Models.Clan
{
    public class TopMainClan : TopClan, IClan
    {
        [JsonProperty("clanPoints")]
        public int ClanPoints { get; internal set; }

    }
}
