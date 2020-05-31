using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace devhl.CocApi.Models.Cache
{
    public class Cache
    {
        public string Path { get; set; } = string.Empty;

        public string Json { get; set; } = string.Empty;

        public string? Key { get; set; }

        public EndPoint EndPoint { get; set; }
    }
}
