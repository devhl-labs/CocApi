using System;
using System.Globalization;
using System.Linq;

namespace CocApi.Rest.Client
{
    public partial class ApiResponse<T>
    {
        public DateTime Downloaded
        {
            get
            {
                string? downloadDateString = Headers.FirstOrDefault(h => h.Key == "Date").Value.FirstOrDefault();

                DateTime downloadDate = downloadDateString == null
                    ? DateTime.UtcNow
                    : DateTime.ParseExact(downloadDateString, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

                return downloadDate;
            }
        }

        public DateTime ServerExpiration
        {
            get
            {
                double maxAge = (Headers.CacheControl?.MaxAge?.TotalSeconds) ?? 0;

                if (maxAge == 0)
                    maxAge = int.Parse(Environment.GetEnvironmentVariable("COCAPI_CACHE_CONTROL_MAX_AGE") ?? "5");

                return Downloaded.AddSeconds(maxAge);
            }
        }
    }
}