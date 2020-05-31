using devhl.CocApi.Models;
using Newtonsoft.Json;
using System;

namespace devhl.CocApi
{
    public abstract class Downloadable : IDownloadable
    {
        [JsonProperty]
        public DateTime DownloadedAtUtc { get; internal set; }

        [JsonProperty]
        public DateTime LocalExpirationUtc { get; internal set; }

        //[JsonProperty]
        //public string EncodedUrl { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime ServerExpirationUtc { get; internal set; } = DateTime.UtcNow;

        public bool IsExpired()
        {
            if (DateTime.UtcNow < LocalExpirationUtc)
                return false;

            if (DateTime.UtcNow < ServerExpirationUtc)
                return false;

            return true;
        }

        public DateTime EffectiveExpiration() => (LocalExpirationUtc > ServerExpirationUtc) ? LocalExpirationUtc : ServerExpirationUtc;
    }
}
