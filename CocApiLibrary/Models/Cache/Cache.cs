using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Security;
using System.Text;

namespace devhl.CocApi.Models.Cache
{
    public class Cache
    {
        public string Path { get; set; } = string.Empty;

        public string Json { get; set; } = string.Empty;

        public EndPoint EndPoint { get; set; }

        public DateTime ServerExpiration { get; set; }

        public Cache()
        {

        }

        public Cache(string path, string json, EndPoint endPoint, DateTime serverExpiration)
        {
            Path = path;

            Json = json;
            
            EndPoint = endPoint;

            ServerExpiration = serverExpiration;
        }
    }
}
