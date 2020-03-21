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
        public DateTime CacheExpiresAtUtc { get; internal set; }

        [JsonProperty]
        public string EncodedUrl { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime? ServerResponseRefreshesAtUtc { get; internal set; }

        public bool IsExpired()
        {
            if (ServerResponseRefreshesAtUtc != null && DateTime.UtcNow < ServerResponseRefreshesAtUtc.Value) return false;

            if (DateTime.UtcNow < CacheExpiresAtUtc) return false;

            return true;
        }
    }
}
