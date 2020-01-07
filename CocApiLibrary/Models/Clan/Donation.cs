using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

#nullable disable

namespace devhl.CocApi.Models.Clan
{
    public class Donation
    {
        [JsonProperty]
        public ClanVillage Village { get; internal set; }

        [JsonProperty]
        public int Quantity { get; internal set; }

        public override string ToString()
        {
            if (Village != null)
            {
                return $"{Village.VillageTag} {Village.Name} {Quantity}";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
