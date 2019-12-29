using System;

namespace devhl.CocApi.Models
{
    public interface IDownloadable
    {
        DateTime UpdatedAtUtc { get; }

        DateTime ExpiresAtUtc { get; }

        string EncodedUrl { get; }

        DateTime? CacheExpiresAtUtc { get; }

        bool IsExpired();
    }
}