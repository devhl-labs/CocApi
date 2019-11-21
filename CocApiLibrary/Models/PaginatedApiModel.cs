using System;
using System.Collections.Generic;

namespace devhl.CocApi.Models
{
    public class PaginatedApiModel<T> : IDownloadable
    {
        public IEnumerable<T>? Items { get; set; }

        public PagingApiModel? Paging { get; set; }

        public DateTime UpdatedAtUtc { get; internal set; } = DateTime.UtcNow;

        public DateTime ExpiresAtUtc { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;

        public DateTime? CacheExpiresAtUtc { get; set; }

        public bool IsExpired()
        {
            if (DateTime.UtcNow > ExpiresAtUtc)
            {
                return true;
            }
            return false;
        }
    }
}
