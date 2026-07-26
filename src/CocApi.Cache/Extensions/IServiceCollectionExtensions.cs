using CocApi.Cache.Services;
using CocApi.Cache.Services.Options;
using CocApi.Cache.DelegatingHandlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScheduledServices.Extensions;
using System;
using System.Linq;

namespace CocApi.Cache.Extensions;

public static class IServiceCollectionExtensions
{
    private static void AddPlayersClient<TPlayersClient>(this IServiceCollection services)
        where TPlayersClient : PlayersClient
    {
        services.AddSingleton<TPlayersClient>();

        if (typeof(TPlayersClient) != typeof(PlayersClient))
            services.AddSingleton(provider =>
            {
                return (PlayersClient)provider.GetRequiredService<TPlayersClient>();
            });
    }

    private static void AddClansClient<TClansClient>(this IServiceCollection services)
        where TClansClient : ClansClient
    {
        services.AddSingleton<TClansClient>();

        if (typeof(TClansClient) != typeof(ClansClient))
            services.AddSingleton(provider =>
            {
                return (ClansClient)provider.GetRequiredService<TClansClient>();
            });
    }

    public static void AddCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> dbContextOptions,
            Action<CacheOptions>? cacheOptions = null)
            where TClansClient : ClansClient
            where TPlayersClient : PlayersClient
            where TTimeToLiveProvider : TimeToLiveProvider
        => services.AddCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(dbContextOptions, null, cacheOptions);

    public static void AddCocApiCache(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> dbContextOptions,
            Action<CacheOptions>? cacheOptions = null)
        => AddCocApiCache<ClansClient, PlayersClient, TimeToLiveProvider>(services, dbContextOptions, null, cacheOptions);

    internal static void AddCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? dbContextOptions,
        Action<IServiceProvider, DbContextOptionsBuilder>? dbContextOptionsBuilderWithServiceCollection,
        Action<CacheOptions>? cacheOptions)
        where TClansClient : ClansClient
        where TPlayersClient : PlayersClient
        where TTimeToLiveProvider : TimeToLiveProvider
    {
        if (!services.Any(x => x.ServiceType == typeof(Rest.Apis.IClansApi)))
            throw new InvalidOperationException("ClansApi was not found in the service collection. Add it using AddCocApi");

        if (!services.Any(x => x.ServiceType == typeof(Rest.Apis.IPlayersApi)))
            throw new InvalidOperationException("PlayersApi was not found in the service collection. Add it using AddCocApi");

        Library.AddStaticJsonOptions(services);

        services.AddTransient<PatchRealTimeResponse>();
        services.AddHttpClient("CocApi.Rest.Apis.IClansApi").AddHttpMessageHandler<PatchRealTimeResponse>();

        services.AddOptions<CacheOptions>().BindConfiguration("CocApi:Cache");
        services.AddOptions<ThreadPoolMonitorServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.ThreadPoolMonitor)}");
        services.AddOptions<ActiveWarServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.ActiveWars)}");
        services.AddOptions<ClanServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.Clans)}");
        services.AddOptions<ClanWarServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.ClanWars)}");
        services.AddOptions<CwlWarServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.CwlWars)}");
        services.AddOptions<MemberServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.ClanMembers)}");
        services.AddOptions<NewCwlWarServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.NewCwlWars)}");
        services.AddOptions<NewWarServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.NewWars)}");
        services.AddOptions<PlayerServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.Players)}");
        services.AddOptions<WarServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.Wars)}");
        services.AddOptions<StalePlayerServiceOptions>().BindConfiguration($"CocApi:Cache:{nameof(CacheOptions.DeleteStalePlayers)}");

        if (cacheOptions != null)
            services.Configure<CacheOptions>(instance => cacheOptions(instance));

        services.AddSingleton<TTimeToLiveProvider>();
        if (typeof(TTimeToLiveProvider) != typeof(TimeToLiveProvider))
            services.AddSingleton(provider =>
            {
                return (TimeToLiveProvider)provider.GetRequiredService<TTimeToLiveProvider>();
            });

        services.AddSingleton<Synchronizer>();
        services.AddSingleton<CachingService>();

        if (dbContextOptions != null)
            services.AddDbContext<CacheDbContext>(dbContextOptions);

        if (dbContextOptionsBuilderWithServiceCollection != null)
            services.AddDbContext<CacheDbContext>(dbContextOptionsBuilderWithServiceCollection);

        services.AddPlayersClient<TPlayersClient>();
        services.AddClansClient<TClansClient>();

        services.AddHostedSingleton<DatabaseValidationService>();
        services.AddHostedSingleton<FireAndForgetService>();
        services.AddHostedSingleton<CacheOptionsMonitorService>();
        services.AddHostedSingleton<ThreadPoolMonitorService>();
        services.AddHostedSingleton<ActiveWarService>();
        services.AddHostedSingleton<ClanService>();
        services.AddHostedSingleton<ClanWarService>();
        services.AddHostedSingleton<CwlWarService>();
        services.AddHostedSingleton<MemberService>();
        services.AddHostedSingleton<NewCwlWarService>();
        services.AddHostedSingleton<NewWarService>();
        services.AddHostedSingleton<PlayerService>();
        services.AddHostedSingleton<WarService>();
        services.AddHostedSingleton<StalePlayerService>();
    }
}
