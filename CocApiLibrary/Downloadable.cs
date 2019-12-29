using devhl.CocApi.Models;
using System;

namespace devhl.CocApi
{
    public abstract class Downloadable : IDownloadable
    {
        public DateTime UpdatedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public string EncodedUrl { get; set; } = string.Empty;

        public DateTime? CacheExpiresAtUtc { get; set; }

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
