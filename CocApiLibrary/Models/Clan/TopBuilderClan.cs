using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.Clan
{
    public class TopBuilderClan : TopClan, IClanApiModel
    {
        [JsonPropertyName("clanVersusPoints")]
        public int ClanVersusPoints { get; set; }

    }
}
