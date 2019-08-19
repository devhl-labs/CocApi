using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Models
{
    public interface IDownloadable
    {
        DateTime DateTimeUTC { get; }

        DateTime Expires { get; }

        string EncodedUrl { get; }

        bool IsExpired();
    }
}
