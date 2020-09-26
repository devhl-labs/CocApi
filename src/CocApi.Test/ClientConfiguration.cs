using System;
using Microsoft.Extensions.DependencyInjection;

using CocApi.Cache;
using Microsoft.EntityFrameworkCore;

namespace CocApi.Test
{
    public class ClientConfiguration : Cache.ClientConfiguration
    {
        // this optional file shows how to use other database providers
        // currently this postgres example is not working 
        // likely due to using the latest .net libraries

        public ClientConfiguration(string connectionString, int concurrentUpdates = 1, TimeSpan? delayBetweenUpdates = null) 
            : base(connectionString, concurrentUpdates, delayBetweenUpdates)
        {
           
        }

        public override IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection()
                .AddDbContext<CacheContext>(o =>
                {
                    o.UseNpgsql(ConnectionString);
                    //o.UseSnakeCaseNamingConvention();  // not currently working
                }).BuildServiceProvider();

            services.GetRequiredService<CacheContext>().Database.Migrate();

            return services;
        }
    }
}