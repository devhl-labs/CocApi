﻿using Newtonsoft.Json;

#nullable disable

namespace devhl.CocApi.Models.Clan
{
    public class Donation
    {
        [JsonProperty]
        public ClanVillage Fetched { get; internal set; }

        [JsonProperty]
        public ClanVillage Queued { get; internal set; }

        public override string ToString()
        {
            if (Fetched != null)
            {
                return $"{Fetched.VillageTag} {Fetched.Name}";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
