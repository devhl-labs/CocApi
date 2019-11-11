using System;

namespace devhl.CocApi.Models
{
    public class LeagueGroupNotFound : ILeagueGroup, IDownloadable
    {
        public DateTime UpdateAtUtc => DateTime.UtcNow;

        public DateTime Expires { get; set; }

        public string EncodedUrl { get; set; } = string.Empty;

        public DateTime? CacheExpiresAtUtc { get; set; }

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
