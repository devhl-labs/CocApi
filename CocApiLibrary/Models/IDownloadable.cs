using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Models
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
