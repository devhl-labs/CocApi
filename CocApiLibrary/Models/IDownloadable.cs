using System;

namespace devhl.CocApi.Models
{
    public interface IDownloadable
    {
        DateTime DownloadedAtUtc { get; }

        DateTime CacheExpiresAtUtc { get; }

        string EncodedUrl { get; }

        DateTime? ServerResponseRefreshesAtUtc { get; }

        bool IsExpired();
    }
}