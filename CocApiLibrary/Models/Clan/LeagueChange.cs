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

        public override string ToString()
        {
            if (Village != null  && League != null)
            {
                return $"{Village.VillageTag} {Village.Name} {League.Name}";
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
