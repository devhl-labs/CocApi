using CocApi.Cache.Services;
using CocApi.Cache.Services.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScheduledServices.Extensions;
using System;
using System.Linq;

namespace CocApi.Cache.Extensions
{
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
            => services.AddCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(dbContextOptions, cacheOptions);

        public static void AddCocApiCache(
                this IServiceCollection services,
                Action<DbContextOptionsBuilder> dbContextOptions,
                Action<CacheOptions>? cacheOptions = null)
            => AddCocApiCache<ClansClient, PlayersClient, TimeToLiveProvider>(services, dbContextOptions, cacheOptions);

        internal static void AddCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder>? dbContextOptions = null,
            Action<IServiceProvider, DbContextOptionsBuilder>? dbContextOptionsBuilderWithServiceCollection = null,
            Action<CacheOptions>? cacheOptions = null)
            where TClansClient : ClansClient
            where TPlayersClient : PlayersClient
            where TTimeToLiveProvider : TimeToLiveProvider
        {
            if (!services.Any(x => x.ServiceType == typeof(Rest.IApis.IClansApi)))
                throw new InvalidOperationException("ClansApi was not found in the service collection. Add it using AddCocApi");

            if (!services.Any(x => x.ServiceType == typeof(Rest.IApis.IPlayersApi)))
                throw new InvalidOperationException("PlayersApi was not found in the service collection. Add it using AddCocApi");

            Library.AddStaticJsonOptions(services);

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

            services.AddHostedSingleton<ActiveWarService>();
            services.AddHostedSingleton<ClanService>();
            services.AddHostedSingleton<CwlWarService>();
            services.AddHostedSingleton<MemberService>();
            services.AddHostedSingleton<NewCwlWarService>();
            services.AddHostedSingleton<NewWarService>();
            services.AddHostedSingleton<PlayerService>();
            services.AddHostedSingleton<WarService>();
            services.AddHostedSingleton<StalePlayerService>();
        }
    }
}
