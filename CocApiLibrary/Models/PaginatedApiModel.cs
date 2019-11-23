using System;
using System.Collections.Generic;

namespace devhl.CocApi.Models
{
    public class PaginatedApiModel<T> : Downloadable, IInitialize /*, IDownloadable*/
    {
        public IEnumerable<T>? Items { get; set; }

        public PagingApiModel? Paging { get; set; }

        public void Initialize()
        {
            foreach(var item in Items.EmptyIfNull())
            {
                if (item is IInitialize initialize)
                {
                    initialize.Initialize();
                }
            }
        }

        //public DateTime UpdatedAtUtc { get; set; }

        //public DateTime ExpiresAtUtc { get; set; }

        //public string EncodedUrl { get; set; } = string.Empty;

        //public DateTime? CacheExpiresAtUtc { get; set; }

        //public bool IsExpired()
        //{
        //    if (DateTime.UtcNow > ExpiresAtUtc)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
    }
}
