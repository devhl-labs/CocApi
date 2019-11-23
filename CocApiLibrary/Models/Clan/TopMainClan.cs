using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.Clan
{
    public class TopMainClan : TopClan, IClanApiModel
    {
        [JsonPropertyName("clanPoints")]
        public int ClanPoints { get; set; }

    }
}
