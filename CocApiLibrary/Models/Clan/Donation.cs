using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi.Models.Clan
{
    public class Donation
    {
        public ClanVillageApiModel Village { get; set; }

        public int Quantity { get; set; }
    }
}
