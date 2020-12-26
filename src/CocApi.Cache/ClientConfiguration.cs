using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public class ClientConfiguration
    {
        public string ConnectionString { get; }

        public TimeSpan DelayBetweenTasks { get; }

        public TimeSpan HttpRequestTimeOut { get; }

        public int ConcurrentClanDownloads { get; }

        public int ConcurrentClanWarDownloads { get; }

        public int ConcurrentCwlDownloads { get; }

        public int ConcurrentPlayerDownloads { get; }

        public int ConcurrentWarLogDownloads { get; }

        public ClientConfiguration(
            string connectionString = "Data Source=CocApi.Cache.sqlite", 
            TimeSpan? delayBetweenTasks = null, 
            TimeSpan? httpRequestTimeOut = null,
            int concurrentClanWarDownloads = 10,
            int concurrentCwlDownloads = 10,
            int concurrentPlayerDownloads = 10,
            int concurrentWarLogDownloads = 10,
            int concurrentClanDownloads = 10)
        {
            ConnectionString = connectionString;

            DelayBetweenTasks = delayBetweenTasks ?? TimeSpan.FromMilliseconds(250);

            HttpRequestTimeOut = httpRequestTimeOut ?? TimeSpan.FromSeconds(5);

            ConcurrentClanDownloads = concurrentClanDownloads;
            ConcurrentClanWarDownloads = concurrentClanWarDownloads;
            ConcurrentCwlDownloads = concurrentCwlDownloads;
            ConcurrentPlayerDownloads = concurrentPlayerDownloads;
            ConcurrentWarLogDownloads = concurrentWarLogDownloads;
        }

        private static IServiceProvider? _services;
        private static readonly object _serviceProviderLock = new object();

        public virtual IServiceProvider BuildServiceProvider()
        {
            lock (_serviceProviderLock)
            {
                if (_services != null)
                    return _services;

                var services = new ServiceCollection()
                    .AddDbContext<CacheContext>(o =>
                        o.UseSqlite(ConnectionString))
                    .BuildServiceProvider();

                CacheContext cachedContext = services.GetRequiredService<CacheContext>();

                if (cachedContext.Database.GetPendingMigrations().Count() > 0)
                {
                    var timeout = cachedContext.Database.GetCommandTimeout();

                    cachedContext.Database.SetCommandTimeout(TimeSpan.FromHours(1));

                    cachedContext.Database.Migrate();

                    cachedContext.Database.SetCommandTimeout(timeout);
                };

                _services = services;

                return _services;
            }
        }
    }
}