using System;

namespace devhl.CocApi.Models
{
    public interface IDownloadable
    {
        DateTime UpdateAtUtc { get; }

        DateTime Expires { get; }

        string EncodedUrl { get; }

        DateTime? CacheExpiresAtUtc { get; set; }

        bool IsExpired();
    }
}
