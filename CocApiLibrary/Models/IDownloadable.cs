using System;

namespace devhl.CocApi.Models
{
    public interface IDownloadable
    {
        DateTime UpdatedAtUtc { get; set; }

        DateTime ExpiresAtUtc { get; set; }

        string EncodedUrl { get; set; }

        DateTime? CacheExpiresAtUtc { get; set; }

        bool IsExpired();
    }

}
//todo make this abstract class