using devhl.CocApi.Models;
using Newtonsoft.Json;
using System;

namespace devhl.CocApi
{
    public abstract class Downloadable : IDownloadable
    {
        [JsonProperty]
        public DateTime DownloadedAtUtc { get; internal set; }

        //[JsonProperty]
        //public DateTime LocalExpirationUtc { get; internal set; }

        //[JsonProperty]
        //public string EncodedUrl { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime ServerExpirationUtc { get; internal set; } = DateTime.UtcNow;

        public bool IsLocallyExpired(TimeSpan additionToServerExpiration) => DateTime.UtcNow > ServerExpirationUtc.Add(additionToServerExpiration);

        public bool ServerResponseIsExpired() => DateTime.UtcNow > ServerExpirationUtc;

        //public DateTime LocalExpiration() => (LocalExpirationUtc > ServerExpirationUtc) ? LocalExpirationUtc : ServerExpirationUtc;
    }
}
