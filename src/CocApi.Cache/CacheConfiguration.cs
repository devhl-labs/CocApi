using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public class CacheConfiguration
    {
        public string ConnectionString { get; }

        public int ConcurrentUpdates { get; }

        public TimeSpan DelayBetweenUpdates { get; }

        public CacheConfiguration(string connectionString = "Data Source=cocapi.db", int concurrentUpdates = 1, TimeSpan? delayBetweenUpdates = null)
        {
            ConnectionString = connectionString;

            ConcurrentUpdates = concurrentUpdates;

            DelayBetweenUpdates = delayBetweenUpdates ?? TimeSpan.FromMilliseconds(50);

            new CachedContext(new DbContextOptionsBuilder().UseSqlite(ConnectionString).Options).Database.Migrate();
        }
    }
}