using System;
using System.Collections.Generic;
using System.Text;
using static devhl.CocApi.Enums;

#nullable disable

namespace devhl.CocApi.Models.Clan
{
    public class RoleChange
    {
        public ClanVillageApiModel Village { get; set; } 
        
        public Role Role { get; set; }
    }
}
