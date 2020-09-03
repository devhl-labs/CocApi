using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public class ClientConfigurationBase
    {
        public string ConnectionString { get; }

        public int ConcurrentUpdates { get; }

        public TimeSpan DelayBetweenUpdates { get; }

        public ClientConfigurationBase(string connectionString = "Data Source=cocapi.db", int concurrentUpdates = 1, TimeSpan? delayBetweenUpdates = null)
        {
            ConnectionString = connectionString;

            ConcurrentUpdates = concurrentUpdates;

            DelayBetweenUpdates = delayBetweenUpdates ?? TimeSpan.FromMilliseconds(50);
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
                    .AddDbContext<CachedContext>(o =>
                        o.UseSqlite(ConnectionString))
                    .BuildServiceProvider();

                services.GetRequiredService<CachedContext>().Database.Migrate();

                _services = services;

                return _services;
            }
        }
    }
}