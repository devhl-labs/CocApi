using CocApi.Cache.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        private static void AddSingletons<TTimeToLiveProvider>(this IServiceCollection services)
            where TTimeToLiveProvider : TimeToLiveProvider
        {
            services.AddSingleton<TTimeToLiveProvider>();
            if (typeof(TTimeToLiveProvider) != typeof(TimeToLiveProvider))
                services.AddSingleton(provider =>
                {
                    return (TimeToLiveProvider)provider.GetRequiredService<TTimeToLiveProvider>();
                });

            services.AddSingleton<Synchronizer>();
            services.AddSingleton<ActiveWarService>();
            services.AddSingleton<ClanService>();
            services.AddSingleton<CwlWarService>();
            services.AddSingleton<MemberService>();
            services.AddSingleton<NewCwlWarService>();
            services.AddSingleton<NewWarService>();
            services.AddSingleton<PlayerService>();
            services.AddSingleton<WarService>();
            services.AddSingleton<StalePlayerService>();
            services.AddSingleton(services => new IPerpetualExecution<object>[]{
                services.GetRequiredService<ActiveWarService>(),
                services.GetRequiredService<ClanService>(),
                services.GetRequiredService<CwlWarService>(),
                services.GetRequiredService<MemberService>(),
                services.GetRequiredService<NewCwlWarService>(),
                services.GetRequiredService<NewWarService>(),
                services.GetRequiredService<PlayerService>(),
                services.GetRequiredService<StalePlayerService>(),
                services.GetRequiredService<WarService>()
            });
        }

        public static void AddCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(
            this IServiceCollection services,
            Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptions,
            Action<CacheOptions>? cacheOptions = null)
            where TClansClient : ClansClient
            where TPlayersClient : PlayersClient
            where TTimeToLiveProvider : TimeToLiveProvider
        {
            if (!services.Any(x => x.ServiceType == typeof(CocApi.Rest.IApis.IClansApi)))
                throw new InvalidOperationException("ClansApi was not found in the service collection. Add it using AddCocApi");

            if (!services.Any(x => x.ServiceType == typeof(CocApi.Rest.IApis.IPlayersApi)))
                throw new InvalidOperationException("PlayersApi was not found in the service collection. Add it using AddCocApi");

            Library.AddStaticJsonOptions(services);

            if (cacheOptions != null)
                services.Configure(cacheOptions);

            services.AddSingletons<TTimeToLiveProvider>();

            services.AddDbContext<CocApi.Cache.CacheDbContext>(dbContextOptions);

            services.AddPlayersClient<TPlayersClient>();

            services.AddClansClient<TClansClient>();

            services.AddHostedService(services => services.GetRequiredService<ActiveWarService>());
            services.AddHostedService(services => services.GetRequiredService<ClanService>());
            services.AddHostedService(services => services.GetRequiredService<CwlWarService>());
            services.AddHostedService(services => services.GetRequiredService<MemberService>());
            services.AddHostedService(services => services.GetRequiredService<NewCwlWarService>());
            services.AddHostedService(services => services.GetRequiredService<NewWarService>());
            services.AddHostedService(services => services.GetRequiredService<PlayerService>());
            services.AddHostedService(services => services.GetRequiredService<WarService>());
            services.AddHostedService(services => services.GetRequiredService<StalePlayerService>());
        }

        public static void AddCocApiCache(
                this IServiceCollection services,
                Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptions,
                Action<CacheOptions>? cacheOptions = null)
            => AddCocApiCache<ClansClient, PlayersClient, TimeToLiveProvider>(services, dbContextOptions, cacheOptions);
    }
}
