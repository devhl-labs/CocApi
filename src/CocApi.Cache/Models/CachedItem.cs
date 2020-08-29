using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Net;
using CocApi.Client;
using Newtonsoft.Json;

namespace CocApi.Cache.Models
{
    public class CachedItem<T> where T : class
    {
        public int Id { get; internal set; }

        public string RawContent { get; internal set; } = string.Empty;

        public DateTime Downloaded { get; internal set; }

        public DateTime ServerExpiration { get; internal set; }

        public DateTime LocalExpiration { get; internal set; }

        public HttpStatusCode StatusCode { get; internal set; }

        private T _data;

        [NotMapped]
        public T Data
        {
            get
            {
                if (_data == null)
                    _data = JsonConvert.DeserializeObject<T>(RawContent, Clash.JsonSerializerSettings);

                return _data;
            }

            internal set { _data = value; }
        }

        public CachedItem()
        {

        }

        public CachedItem(ApiResponse<T> response, TimeSpan localExpiration)
        {
            string downloadDateString = response.Headers.First(h => h.Key == "Date").Value.First();
            DateTime downloadDate = DateTime.ParseExact(downloadDateString, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture);
            string cacheControlString = response.Headers.First(h => h.Key == "Cache-Control").Value.First().Replace("max-age=", "");
            double cacheControl = double.Parse(cacheControlString);
            DateTime serverExpiration = downloadDate.AddSeconds(cacheControl);

            Downloaded = downloadDate;
            RawContent = response.RawContent;
            ServerExpiration = serverExpiration;
            Data = response.Data;  
            LocalExpiration = Downloaded.Add(localExpiration);
            StatusCode = response.StatusCode;
        }

        protected void UpdateFrom(ApiResponse<T> apiResponse, TimeSpan localExpiration)
        {
            StatusCode = apiResponse.StatusCode;

            RawContent = apiResponse?.RawContent ?? RawContent;

            Downloaded = apiResponse?.Downloaded ?? DateTime.UtcNow;

            ServerExpiration = apiResponse?.ServerExpiration ?? DateTime.UtcNow;

            LocalExpiration = Downloaded.Add(localExpiration);

            Data = apiResponse?.Data ?? Data;
        }

        protected void UpdateFrom(ApiException apiException, TimeSpan localExpiration)
        {
            StatusCode = (HttpStatusCode) apiException.ErrorCode;

            Downloaded = DateTime.UtcNow;

            ServerExpiration = DateTime.UtcNow;

            LocalExpiration = DateTime.UtcNow.Add(localExpiration);
        }

        public bool IsServerExpired() => DateTime.UtcNow > ServerExpiration.AddSeconds(15);

        public bool IsLocallyExpired() => DateTime.UtcNow > LocalExpiration;
    }
}
