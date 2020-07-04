using System;

namespace devhl.CocApi.Models
{
    public interface IDownloadable
    {
        DateTime DownloadedAtUtc { get; }

        DateTime ServerExpirationUtc { get; }

        bool IsLocallyExpired(TimeSpan additionToServerExpiration);

        bool ServerResponseIsExpired();

    }
}