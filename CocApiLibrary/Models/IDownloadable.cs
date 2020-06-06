using System;

namespace devhl.CocApi.Models
{
    public interface IDownloadable
    {
        DateTime DownloadedAtUtc { get; }

        //DateTime LocalExpirationUtc { get; }

        //string EncodedUrl { get; }

        DateTime ServerExpirationUtc { get; }

        bool IsLocallyExpired(TimeSpan additionToServerExpiration);

        //DateTime LocalExpiration();

        bool ServerResponseIsExpired();

    }
}