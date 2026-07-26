using System;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CocApi.Cache.Services.Options;

namespace CocApi.Cache.Extensions;

public static class IHostBuilderExtensions
{
    public static IHostBuilder ConfigureCocApiCache(
        this IHostBuilder builder,
        Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptions,
        Action<CacheOptions>? cacheOptions = null)
        => ConfigureCocApiCache<ClansClient, PlayersClient, TimeToLiveProvider>(builder, dbContextOptions, cacheOptions);

    public static IHostBuilder ConfigureCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(
        this IHostBuilder builder,
        Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptions,
        Action<CacheOptions>? cacheOptions = null)
        where TClansClient : ClansClient
        where TPlayersClient : PlayersClient
        where TTimeToLiveProvider : TimeToLiveProvider
    {
        builder.ConfigureServices((_, services) =>
            IServiceCollectionExtensions.AddCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(
                services,
                null,
                dbContextOptions,
                cacheOptions));

        return builder;
    }
}
