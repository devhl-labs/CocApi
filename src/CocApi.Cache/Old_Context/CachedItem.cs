using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace CocApi.Cache.Models
{
    public class CachedItem<T> where T : class
    {
        public int Id { get; internal set; }

        public string? RawContent { get; internal set; }

        public DateTime Downloaded { get; internal set; }

        public DateTime ServerExpiration { get; internal set; }

        public DateTime LocalExpiration { get; internal set; }

        public HttpStatusCode StatusCode { get; internal set; }

        private T? _data;

        private readonly object _dataLock = new();

        [NotMapped]
        public T? Data
        {
            get
            {
                lock(_dataLock)
                    if (_data == null && !string.IsNullOrWhiteSpace(RawContent))
                    {
                        _data = System.Text.Json.JsonSerializer.Deserialize<T>(RawContent, Library.JsonSerializerOptions);

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

        public CachedItem(ApiResponse<T> apiResponse, TimeSpan localExpiration) => UpdateFrom(apiResponse, localExpiration);

        public CachedItem(TimeSpan localExpiration) => UpdateFrom(localExpiration);
        
        protected void UpdateFrom(ApiResponse<T> apiResponse, TimeSpan localExpiration)
        {
            StatusCode = apiResponse.StatusCode;
            Downloaded = apiResponse.DownloadedAt;
            ServerExpiration = apiResponse.ServerExpiration;

            LocalExpiration = localExpiration == TimeSpan.MaxValue
                ? DateTime.MaxValue
                : DateTime.UtcNow.Add(localExpiration);

            if (apiResponse.IsSuccessStatusCode)
            {
                RawContent = apiResponse.RawContent;
                Data = apiResponse.Content;
            }
        }

        protected void UpdateFrom(TimeSpan localExpiration)
        {
            StatusCode = 0;
            Downloaded = DateTime.UtcNow;
            ServerExpiration = DateTime.UtcNow;
            LocalExpiration = localExpiration == TimeSpan.MaxValue
                ? DateTime.MaxValue
                : DateTime.UtcNow.Add(localExpiration);
        }

        protected void UpdateFrom(CachedItem<T> fetched)
        {
            StatusCode = fetched.StatusCode;
            Downloaded = fetched.Downloaded;
            ServerExpiration = fetched.ServerExpiration;
            LocalExpiration = fetched.LocalExpiration;

            Data = fetched.Data ?? _data;
            RawContent = (!string.IsNullOrWhiteSpace(fetched.RawContent)) ? fetched.RawContent : RawContent;
        }

        public bool IsServerExpired() => DateTime.UtcNow > ServerExpiration.AddSeconds(3);

        public bool IsLocallyExpired() => DateTime.UtcNow > LocalExpiration;
    }
}
