using System;
using System.Collections.Generic;
using System.Text;
////System.Text.Json.Serialization
using Newtonsoft.Json;

namespace devhl.CocApi.Models.Clan
{
    public class TopMainClan : TopClan, IClanApiModel
    {
        [JsonProperty("clanPoints")]
        public int ClanPoints { get; set; }

    }
}
