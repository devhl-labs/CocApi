using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Models
{
    public interface IDownloadable
    {
        DateTime DateTimeUtc { get; }

        DateTime Expires { get; }

        string EncodedUrl { get; }

        bool IsExpired();
    }
}
