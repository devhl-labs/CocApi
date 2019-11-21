using System;

namespace devhl.CocApi.Models
{
    public class LeagueGroupNotFound : ILeagueGroup, IDownloadable
    {
        public DateTime UpdatedAtUtc => DateTime.UtcNow;

        public DateTime ExpiresAtUtc { get; set; }

        public string EncodedUrl { get; set; } = string.Empty;

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
