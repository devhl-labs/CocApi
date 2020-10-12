using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Net;
using CocApi.Client;
using CocApi.Model;
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

        private T? _data;

        [NotMapped]
        public T? Data
        {
            get
            {
                if (_data == null)
                {
                    _data = JsonConvert.DeserializeObject<T>(RawContent, Clash.JsonSerializerSettings);

                    if (_data is ClanWar clanWar)
                        if (this is CachedWar cachedWar)
                            clanWar.Initialize(ServerExpiration, cachedWar.WarTag);
                        else
                            clanWar.Initialize(ServerExpiration, null);
                }

                return _data;
            }

            internal set { _data = value; }
        }

        public CachedItem()
        {

        }

        public CachedItem(ApiResponse<T> apiResponse, TimeSpan localExpiration)
        {
            UpdateFrom(apiResponse, localExpiration);
        }

        public CachedItem(Exception exception, TimeSpan localExpiration)
        {
            UpdateFrom(exception, localExpiration);
        }

        protected void UpdateFrom(ApiResponse<T> apiResponse, TimeSpan localExpiration)
        {
            StatusCode = apiResponse.StatusCode;

            //if (apiResponse.Data != null)
            //    if (apiResponse.Data is ClanWar)
            //        RawContent = JsonConvert.SerializeObject(apiResponse.Data);  //adding war type and server expiration to the data
            //    else
            RawContent = apiResponse?.RawContent ?? RawContent;

            Downloaded = apiResponse?.Downloaded ?? DateTime.UtcNow;
            ServerExpiration = apiResponse?.ServerExpiration ?? DateTime.UtcNow;
            LocalExpiration = Downloaded.Add(localExpiration);
            Data = apiResponse?.Data ?? Data;
        }

        protected void UpdateFrom(Exception e, TimeSpan localExpiration)
        {
            if (e is ApiException apiException)
                UpdateFrom(apiException, localExpiration);
            else if (e is TimeoutException timeout)
                UpdateFrom(timeout, localExpiration);
            else
                throw new NotImplementedException();
        }

        protected void UpdateFrom(ApiException apiException, TimeSpan localExpiration)
        {
            StatusCode = (HttpStatusCode) apiException.ErrorCode;
            Downloaded = DateTime.UtcNow;
            ServerExpiration = DateTime.UtcNow;
            LocalExpiration = DateTime.UtcNow.Add(localExpiration);
        }

        protected void UpdateFrom(TimeoutException timeoutException, TimeSpan localExpiration)
        {
            StatusCode = 0;
            Downloaded = DateTime.UtcNow;
            ServerExpiration = DateTime.UtcNow;
            LocalExpiration = DateTime.UtcNow.Add(localExpiration);
        }

        protected void UpdateFrom(CachedItem<T> fetched)
        {
            StatusCode = fetched.StatusCode;

            //if (fetched.Data != null)
            //    if (fetched.Data is ClanWar)
            //        RawContent = JsonConvert.SerializeObject(fetched.Data); //adding war type and server expiration to the data
            //    else
            RawContent = (!string.IsNullOrEmpty(fetched.RawContent)) ? fetched.RawContent : RawContent;

            Downloaded = fetched.Downloaded;
            ServerExpiration = fetched.ServerExpiration;
            LocalExpiration = fetched.LocalExpiration;
            Data = fetched.Data ?? _data;
        }

        public bool IsServerExpired() => DateTime.UtcNow > ServerExpiration.AddSeconds(3);

        public bool IsLocallyExpired() => DateTime.UtcNow > LocalExpiration;
    }
}
