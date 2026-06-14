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
        /// <summary>
        /// The last 8 characters of the API token (Bearer value) used in the request.
        /// Useful for diagnosing 403 Forbidden errors caused by token/IP allowlist mismatches.
        /// The full token is never stored — only the suffix is kept for log identification.
        /// </summary>
        public string? RequestTokenSuffix { get; private set; }

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
            if (httpRequestMessage.Headers.TryGetValues("authorization", out var authValues))
            {
                var raw = authValues.FirstOrDefault();
                if (raw != null)
                    RequestTokenSuffix = raw.Length > 8 ? "..." + raw[^8..] : raw;
            }

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
