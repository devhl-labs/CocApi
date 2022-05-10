using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using CocApi.Rest.Client;
using CocApi.Rest.Models;

namespace CocApi.Cache.Context
{
    public class CachedItem<T> where T : class
    {
        public string? RawContent { get; internal set; }

        public DateTime? DownloadedAt { get; internal set; }

        public DateTime? ExpiresAt { get; internal set; }

        public DateTime? KeepUntil { get; internal set; }

        public HttpStatusCode? StatusCode { get; internal set; }

        public bool Download { get; set; } = true;

        private T? _content;

        private readonly object _contentLock = new();

        [NotMapped]
        public T? Content
        {
            get
            {
                lock (_contentLock)
                    if (_content == null && !string.IsNullOrWhiteSpace(RawContent))
                    {
                        _content = System.Text.Json.JsonSerializer.Deserialize<T>(RawContent, Library.JsonSerializerOptions);

                        if (_content is ClanWar clanWar)
                            if (this is CachedWar cachedWar)
                                clanWar.Initialize(ExpiresAt.Value, cachedWar.WarTag);
                            else
                                clanWar.Initialize(ExpiresAt.Value, null);
                    }

                return _content;
            }

            internal set { _content = value; }
        }

        public CachedItem()
        {

        }

        public CachedItem(ApiResponse<T> apiResponse, TimeSpan localExpiration) => UpdateFrom(apiResponse, localExpiration);

        public CachedItem(TimeSpan localExpiration) => UpdateFrom(localExpiration);

        protected void UpdateFrom(ApiResponse<T?> apiResponse, TimeSpan localExpiration)
        {
            StatusCode = apiResponse.StatusCode;
            DownloadedAt = apiResponse.Downloaded;
            ExpiresAt = apiResponse.ServerExpiration;

            KeepUntil = localExpiration == TimeSpan.MaxValue
                ? DateTime.MaxValue
                : DateTime.UtcNow.Add(localExpiration);

            if (apiResponse.IsSuccessStatusCode)
            {
                RawContent = apiResponse.RawContent;
                Content = apiResponse.Content;
            }
        }

        protected void UpdateFrom(TimeSpan localExpiration)
        {
            StatusCode = 0;
            DownloadedAt = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow;
            KeepUntil = localExpiration == TimeSpan.MaxValue
                ? DateTime.MaxValue
                : DateTime.UtcNow.Add(localExpiration);
        }

        protected void UpdateFrom(CachedItem<T?> fetched)
        {
            StatusCode = fetched.StatusCode;
            DownloadedAt = fetched.DownloadedAt;
            ExpiresAt = fetched.ExpiresAt;
            KeepUntil = fetched.KeepUntil;

            Content = fetched.Content ?? _content;
            RawContent = !string.IsNullOrWhiteSpace(fetched.RawContent) ? fetched.RawContent : RawContent;
        }

        private bool IsServerExpired
        {
            get
            {
                return DateTime.UtcNow > (ExpiresAt ?? DateTime.MinValue).AddSeconds(3);
            }
        }

        private bool IsLocallyExpired
        {
            get
            {
                return DateTime.UtcNow > (KeepUntil ?? DateTime.MinValue);
            }
        }

        public bool IsExpired
        {
            get
            {
                return IsServerExpired && IsLocallyExpired;
            }
        }
    }
}
