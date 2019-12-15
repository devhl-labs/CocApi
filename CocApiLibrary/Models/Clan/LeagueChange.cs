using devhl.CocApi.Models.Village;
using System;
using System.Collections.Generic;
using System.Text;

#nullable disable

namespace devhl.CocApi.Models.Clan
{
    public class LeagueChange
    {
        public ClanVillageApiModel Village { get; set; }
        
        public VillageLeagueApiModel League { get; set; }
    }
}
