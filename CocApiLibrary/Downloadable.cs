using devhl.CocApi.Models;
using System;

namespace devhl.CocApi
{
    public abstract class Downloadable : IDownloadable
    {
        public DateTime UpdatedAtUtc { get; internal set; }

        public DateTime ExpiresAtUtc { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;

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
