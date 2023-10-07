using CocApi.Rest.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace CocApi.Rest.Client
{
    public partial interface IApiResponse
    {
        DateTime Downloaded { get; }

        DateTime ServerExpiration { get; }
    }

    public partial class ApiResponse
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

        partial void OnCreated(HttpRequestMessage httpRequestMessage, HttpResponseMessage httpResponseMessage)
        {
            string? url = httpRequestMessage.RequestUri?.LocalPath;

            if (this is IOk<ClanWar>)
            {
               string serverExpiration = System.Text.Json.JsonSerializer.Serialize(ServerExpiration, _jsonSerializerOptions);
               RawContent = RawContent[..^1];
               RawContent = $"{RawContent}, \"serverExpiration\": {serverExpiration}";

               if (url?.Contains("clanwarleagues/wars/") == true)
               {
                   string[] parts = url.Split("/");
                   RawContent = $"{RawContent}, \"warTag\": \"{parts.Last()}\"";

                   if (!RawContent.Contains("attacksPerMember"))
                       RawContent = $"{RawContent}, \"attacksPerMember\": 1";
               }

               RawContent = $"{RawContent}}}";
            }
        }
    }
}
