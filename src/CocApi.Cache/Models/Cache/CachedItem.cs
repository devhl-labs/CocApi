using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Net;
using CocApi.Client;
using Newtonsoft.Json;

namespace CocApi.Cache.Models.Cache
{
    public class CachedItem<T> where T : class
    {
        public int Id { get; set; }

        public string RawContent { get; set; } = string.Empty;

        public DateTime Downloaded { get; set; }

        public DateTime ServerExpiration { get; set; }

        public DateTime LocalExpiration { get; set; }

        public HttpStatusCode StatusCode { get; set; }

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

        protected void UpdateFromResponse(ApiResponse<T>? responseItem, TimeSpan localExpiration)
        {
            RawContent = responseItem?.RawContent ?? RawContent;

            Downloaded = responseItem?.Downloaded ?? DateTime.UtcNow;

            ServerExpiration = responseItem?.ServerExpiration ?? DateTime.UtcNow;

            LocalExpiration = responseItem?.LocalExpiration(localExpiration) ?? DateTime.UtcNow.Add(localExpiration);

            Data = responseItem?.Data ?? Data;
        }

        protected void UpdateFromResponse(ApiException apiException, TimeSpan localExpiration)
        {
            if (string.IsNullOrEmpty(RawContent))
                RawContent = apiException.ErrorContent.ToString();

            StatusCode = (HttpStatusCode) apiException.ErrorCode;

            Downloaded = DateTime.UtcNow;

            ServerExpiration = DateTime.UtcNow;

            LocalExpiration = DateTime.UtcNow.Add(localExpiration);
        }

        public bool IsServerExpired() => DateTime.UtcNow > ServerExpiration.AddSeconds(3);

        public bool IsLocallyExpired() => DateTime.UtcNow > LocalExpiration;
    }
}
