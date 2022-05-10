using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace CocApi.Cache.Extensions
{
    public static class IHostBuilderExtensions
    {
        public static IHostBuilder ConfigureCocApiCache(
            this IHostBuilder builder,
            Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptions,
            Action<CacheOptions, HostBuilderContext>? cacheOptions = null)
            => ConfigureCocApiCache<ClansClient, PlayersClient, TimeToLiveProvider>(builder, dbContextOptions, cacheOptions);

        public static IHostBuilder ConfigureCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(
            this IHostBuilder builder,
            Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptions,
            Action<CacheOptions, HostBuilderContext>? cacheOptions = null) 
            where TClansClient : ClansClient
            where TPlayersClient : PlayersClient
            where TTimeToLiveProvider : TimeToLiveProvider
        {
            builder.ConfigureServices((context, services) =>
            {
                if (cacheOptions != null)
                    services.AddOptions<CacheOptions>().Configure<HostBuilderContext>((a, b) => cacheOptions(a, b));

                IServiceCollectionExtensions.AddCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(services, dbContextOptions, null);
            });

            return builder;
        }
    }
}
