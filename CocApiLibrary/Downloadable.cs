using devhl.CocApi.Models;
using Newtonsoft.Json;
using System;

namespace devhl.CocApi
{
    public abstract class Downloadable : IDownloadable
    {
        [JsonProperty]
        public DateTime UpdatedAtUtc { get; internal set; }

        [JsonProperty]
        public DateTime ExpiresAtUtc { get; internal set; }

        [JsonProperty]
        public string EncodedUrl { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime? CacheExpiresAtUtc { get; internal set; }

        public bool IsExpired()
        {
            if (CacheExpiresAtUtc.HasValue && DateTime.UtcNow < CacheExpiresAtUtc.Value)
            {
                return false;
            }

            if (DateTime.UtcNow < ExpiresAtUtc)
            {
                return false;
            }

            return true;
        }
    }
}
