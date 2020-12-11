using System;
using Microsoft.Extensions.DependencyInjection;

using CocApi.Cache;
using Microsoft.EntityFrameworkCore;

namespace CocApi.Test
{
    public class ClientConfiguration : Cache.ClientConfiguration
    {
        public ClientConfiguration(string connectionString, TimeSpan? delayBetweenUpdates = null) 
            : base(connectionString, delayBetweenUpdates)
        {
           
        }

        public override IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection()
                .AddDbContext<CacheContext>(o =>
                {
                    o.UseNpgsql(ConnectionString);
                }).BuildServiceProvider();

            services.GetRequiredService<CacheContext>().Database.Migrate();

            return services;
        }
    }
}