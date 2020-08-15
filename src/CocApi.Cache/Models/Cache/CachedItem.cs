using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Security;
using System.Text;
using CocApi.Client;
using Newtonsoft.Json;
using SQLitePCL;

namespace CocApi.Cache.Models.Cache
{
    public class CachedItem_old
    {
        public int Id { get; set; }

        public string Path { get; set; } = string.Empty;

        public string Json { get; set; } = string.Empty;

        public EndPoint EndPoint { get; set; }

        public DateTime ServerExpiration { get; set; }

        public CachedItem_old()
        {

        }

        public CachedItem_old(string path, string json, EndPoint endPoint, DateTime serverExpiration)
        {
            Path = path;

            Json = json;
            
            EndPoint = endPoint;

            ServerExpiration = serverExpiration;
        }
    }















    public class CachedItem
    {
        public int Id { get; set; }

        public string Path { get; set; } = string.Empty;

        public string Raw { get; set; } = string.Empty;

        public DateTime DownloadDate { get; set; }

        public DateTime ServerExpirationDate { get; set; }

        public DateTime LocalExpirationDate { get; set; }

        public CachedItem()
        {

        }

        public CachedItem(string path, string raw, DateTime downloadDate, DateTime serverExpirationDate, DateTime localExpirationDate)
        {
            Path = path;

            Raw = raw;

            DownloadDate = downloadDate;

            ServerExpirationDate = serverExpirationDate;

            LocalExpirationDate = localExpirationDate;
        }

        public bool IsServerExpired() => DateTime.UtcNow > ServerExpirationDate.AddSeconds(1);
        

        public bool IsLocallyExpired() => DateTime.UtcNow > LocalExpirationDate;
        
    }
}
