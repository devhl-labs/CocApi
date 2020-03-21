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
        public League OldLeague { get; internal set; }

        public override string ToString()
        {
            if (Village != null  && OldLeague != null)
            {
                return $"{Village.VillageTag} {Village.Name} {OldLeague.Name}";
            }
            else if (Village != null)
            {
                return $"{Village.VillageTag} {Village.Name}";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
