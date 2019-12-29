using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static devhl.CocApi.Enums;

#nullable disable

namespace devhl.CocApi.Models.Clan
{
    public class RoleChange
    {
        [JsonProperty]
        public ClanVillage Village { get; internal set; } 
        
        [JsonProperty]
        public Role Role { get; internal set; } //todo is this the old role or the new role?
    }
}
