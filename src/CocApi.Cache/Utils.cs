using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    internal static class Utils
    {
        private static IServiceProvider? _services;
        private static readonly object _serviceProviderLock = new object();

        public static IServiceProvider BuildServiceProvider(string connectionString)
        {
            lock (_serviceProviderLock)
            {
                if (_services != null)
                    return _services;

                var services = new ServiceCollection()
                    .AddDbContext<CacheContext>(o =>
                        o.UseSqlite(connectionString))
                    .BuildServiceProvider();

                _services = services;

                return _services;
            }
        }
    }
}
