using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Models
{
    public class NotInWar : IWar
    {
        public DateTime UpdateAtUtc { get; set; } = DateTime.UtcNow;

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
