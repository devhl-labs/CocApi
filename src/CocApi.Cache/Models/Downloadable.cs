using CocApi.Cache.Models;
using Newtonsoft.Json;
using System;

namespace CocApi.Cache
{
    public abstract class Downloadable : IDownloadable
    {
        [JsonProperty]
        public DateTime DownloadedAtUtc { get; internal set; }

        [JsonProperty]
        public DateTime ServerExpirationUtc { get; internal set; } = DateTime.UtcNow;

        public bool IsLocallyExpired(TimeSpan additionToServerExpiration) => DateTime.UtcNow > ServerExpirationUtc.Add(additionToServerExpiration);

        public bool ServerResponseIsExpired() => DateTime.UtcNow > ServerExpirationUtc;
    }
}
