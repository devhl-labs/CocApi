using System;using System.Globalization;using System.Linq;
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
        public DateTime ServerExpiration        {
            get
            {
                var cacheControlString = Headers.FirstOrDefault(h => h.Key == "Cache-Control").Value.FirstOrDefault();
                if (cacheControlString == null)
                {
                    string? envVar = Environment.GetEnvironmentVariable("COCAPI_CURRENT_WAR_CACHE_CONTROL") ?? "5";
                    return DateTime.UtcNow.AddSeconds(int.Parse(envVar));
                }
                cacheControlString = cacheControlString.Replace("public ", "").Replace("max-age=", "");
                double cacheControl = double.Parse(cacheControlString);
                return Downloaded.AddSeconds(cacheControl);
            }
        }
    }
}




