using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using CocApi.Cache.Models;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedPlayer : CachedItem<Player>
    {
        public string Tag { get; set; } = string.Empty;

        public bool Download { get; set; }

        public CachedPlayer(ApiResponse<Player> response, TimeSpan localExpiration) : base (response, localExpiration)
        {
            Tag = response.Data.Tag;
        }

        public CachedPlayer()
        {
        }

        internal void UpdateFromResponse(ApiResponse<Player> apiResponse, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiResponse, localExpiration);
        }
    }
}
