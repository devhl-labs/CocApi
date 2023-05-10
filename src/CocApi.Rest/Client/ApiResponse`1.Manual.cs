using CocApi.Rest.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;

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

        partial void OnCreated(HttpRequestMessage httpRequestMessage, HttpResponseMessage httpResponseMessage)
        {
            if (ResponseType == typeof(ClanWar))
            {
                if (ToModel() as object is not ClanWar clanWar)
                    return;

                clanWar.ServerExpiration = ServerExpiration;
                string? url = httpRequestMessage.RequestUri?.LocalPath;
                if (url?.Contains("clanwarleagues/wars/") == true)
                {
                    string[] parts = url.Split("/");
                    clanWar.WarTag = parts.Last();
                }

                RawContent = System.Text.Json.JsonSerializer.Serialize(clanWar, _jsonSerializerOptions);
            }
        }

        /// <summary>
        /// Deserializes the server's response
        /// </summary>
        public T? ToModel(System.Text.Json.JsonSerializerOptions? options = null)
        {
            if (ResponseType == typeof(ClanWar) && RawContent.Contains("notInWar"))
                return default;

            return IsSuccessStatusCode
                ? System.Text.Json.JsonSerializer.Deserialize<T>(RawContent, options ?? _jsonSerializerOptions)
                : default;
        }
    }
}
