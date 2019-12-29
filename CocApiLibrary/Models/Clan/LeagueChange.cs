using devhl.CocApi.Models.Village;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

#nullable disable

namespace devhl.CocApi.Models.Clan
{
    public class LeagueChange
    {
        [JsonProperty]
        public ClanVillage Village { get; internal set; }
        
        [JsonProperty]
        public League League { get; internal set; }
    }
}
