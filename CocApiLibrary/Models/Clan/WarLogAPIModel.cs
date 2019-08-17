using CocApiLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class WarLogAPIModel : IDownloadable
    {
        public IEnumerable<WarLogEntryModel>? Items { get; set; }

        public PagingAPIModel? Paging { get; set; }

        public DateTime DateTimeUTC { get; internal set; } = DateTime.UtcNow;

        public DateTime Expires { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;

        public bool IsExpired()
        {
            if (DateTime.UtcNow > Expires)
            {
                return true;
            }
            return false;
        }
    }
}
