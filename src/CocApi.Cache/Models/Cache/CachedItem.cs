using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using CocApi.Client;
using Newtonsoft.Json;

namespace CocApi.Cache.Models.Cache
{
    public class CachedItem<T> where T : class
    {
        public int Id { get; set; }

        public string? RawContent { get; set; }

        public DateTime Downloaded { get; set; }

        public DateTime ServerExpiration { get; set; }

        public DateTime LocalExpiration { get; set; }

        private T? _data;

        [NotMapped]
        public T? Data
        {
            get
            {
                if (RawContent == null)
                    return null;

                if (_data == null)
                    _data = JsonConvert.DeserializeObject<T>(RawContent);

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
        }

        protected void UpdateFromResponse(ApiResponse<T> responseItem, TimeSpan localExpiration)
        {
            RawContent = responseItem.RawContent;

            Downloaded = responseItem.Downloaded;

            ServerExpiration = responseItem.ServerExpiration;

            LocalExpiration = responseItem.LocalExpiration(localExpiration);

            Data = responseItem.Data;
        }

        public bool IsServerExpired() => DateTime.UtcNow > ServerExpiration.AddSeconds(1);

        public bool IsLocallyExpired() => DateTime.UtcNow > LocalExpiration;
    }
}
