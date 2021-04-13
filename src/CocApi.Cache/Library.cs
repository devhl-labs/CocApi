using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

namespace CocApi.Cache
{        
    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
        None
    }

    [Flags]
    public enum Announcements
    {
        None = 0,
        WarStartingSoon = 1,
        WarStarted = 2,
        WarEndingSoon = 4,
        WarEnded = 8,
        WarEndNotSeen = 16,
    }

    public static class Library
    {
        internal static void OnLog(object sender, LogEventArgs log)
        {
            try
            {
                Log?.Invoke(sender, log).ConfigureAwait(false);
            }
            catch (Exception)
            {
                //throw;
            }
        }

        public static event LogEventHandler? Log;

        internal static SemaphoreSlim ConcurrentEventsSemaphore = new(25, 25);

        public static class TableNames
        {
            public static string Clans { get; set; } = "clan";

            public static string CurrentWar { get; set; } = "current_war";

            public static string WarLog { get; set; } = "war_log";

            public static string Group { get; set; } = "group";

            public static string Player { get; set; } = "player";

            public static string War { get; set; } = "war";
        }




        private static void AddCocApiDbContext(this IServiceCollection services, Action<CacheDbContextFactoryProvider> provider)
        {
            CacheDbContextFactoryProvider instance = new();

            provider(instance);

            if (instance.Factory == null)
                throw new ArgumentNullException(nameof(instance.Factory), "The database context factory cannot be null. This object is used to connect to your database.");

            services.AddSingleton(instance);
        }

        //private static IHostBuilder ConfigureCocApiDbContext(this IHostBuilder builder, Action<CacheDbContextFactoryProvider> provider)
        //{
        //    builder.ConfigureServices((context, services) => AddCocApiDbContext(services, provider));

        //    return builder;
        //}









        //private static void AddPlayersClient(this IServiceCollection services, Action<MonitorOptions>? options = null)
        //    => AddPlayersClient<PlayersClientBase>(services, options);

        private static void AddPlayersClient<TPlayersClient>(this IServiceCollection services, Action<MonitorOptions>? options) 
            where TPlayersClient : PlayersClientBase
        {
            if (options != null)
                services.Configure(options);

            services.AddSingleton<TPlayersClient>();

            if (typeof(TPlayersClient) != typeof(PlayersClientBase))
                services.AddSingleton(provider =>
                {
                    return (PlayersClientBase)provider.GetRequiredService<TPlayersClient>();
                });
        }

        //private static IHostBuilder ConfigurePlayersClient(this IHostBuilder builder, Action<MonitorOptions>? options = null)
        //    => ConfigurePlayersClient<PlayersClientBase>(builder, options);

        //private static IHostBuilder ConfigurePlayersClient<TPlayersClient>(this IHostBuilder builder, Action<MonitorOptions>? options = null)
        //where TPlayersClient : PlayersClientBase
        //{
        //    builder.ConfigureServices((context, services) => AddPlayersClient<TPlayersClient>(services, options));

        //    return builder;
        //}













        //private static void AddClansClient(this IServiceCollection services, Action<ClanClientOptions>? clanClientOptions = null)
        //    => AddClansClient<ClansClientBase>(services, clanClientOptions);

        private static void AddClansClient<TClansClient>(this IServiceCollection services, Action<ClanClientOptions>? clanClientOptions)
            where TClansClient : ClansClientBase
        {
            if (clanClientOptions != null)
                services.Configure(clanClientOptions);

            services.AddSingleton<TClansClient>();

            if (typeof(TClansClient) != typeof(ClansClientBase))
                services.AddSingleton(provider =>
                {
                    return (ClansClientBase)provider.GetRequiredService<TClansClient>();
                });

            services.AddHostedService((serviceProvider) =>
            {
                ClansClientBase clansClient = serviceProvider.GetRequiredService<TClansClient>();

                return clansClient;
            });
        }

        //private static IHostBuilder ConfigureClansClient(this IHostBuilder builder, Action<ClanClientOptions>? clanClientOptions = null)
        //    => ConfigureClansClient<ClansClientBase>(builder, clanClientOptions);

        //private static IHostBuilder ConfigureClansClient<TClansClient>(this IHostBuilder builder, Action<ClanClientOptions>? clanClientOptions = null)
        //    where TClansClient : ClansClientBase
        //{
        //    builder.ConfigureServices((context, services) => AddClansClient<TClansClient>(services, clanClientOptions));

        //    return builder;
        //}







        public static void AddCocApiCache(this IServiceCollection services,
            Action<CacheDbContextFactoryProvider> provider,
            Action<ClanClientOptions>? clanClientOptions = null, 
            Action<MonitorOptions>? playerClientOptions = null)
            => AddCocApiCache<ClansClientBase, PlayersClientBase>(services, provider, clanClientOptions, playerClientOptions);

        public static void AddCocApiCache<TClansClient, TPlayersClient>(this IServiceCollection services, 
            Action<CacheDbContextFactoryProvider> provider,
            Action<ClanClientOptions>? clanClientOptions = null,
            Action<MonitorOptions>? playerClientOptions = null) 
            where TClansClient : ClansClientBase
            where TPlayersClient : PlayersClientBase
        {
            if (!services.Any(x => x.ServiceType == typeof(CocApi.Api.ClansApi)) ||
                !services.Any(x => x.ServiceType == typeof(CocApi.Api.PlayersApi)))
                throw new InvalidOperationException("ClansApi or PlayersApi were not found in the service collection.");

            if (provider == null)
                throw new InvalidOperationException("The DbContext provider was null.");

            services.AddCocApiDbContext(provider);

            services.AddPlayersClient<TPlayersClient>(playerClientOptions);

            services.AddClansClient<TClansClient>(clanClientOptions);
        }

        public static IHostBuilder ConfigureCocApiCache(this IHostBuilder builder,
            Action<CacheDbContextFactoryProvider> provider,
            Action<ClanClientOptions>? clanClientOptions = null,
            Action<MonitorOptions>? playerClientOptions = null)
            => ConfigureCocApiCache<ClansClientBase, PlayersClientBase>(builder, provider, clanClientOptions, playerClientOptions);

        public static IHostBuilder ConfigureCocApiCache<TClansClient, TPlayersClient>(this IHostBuilder builder,
            Action<CacheDbContextFactoryProvider> provider,
            Action<ClanClientOptions>? clanClientOptions = null,
            Action<MonitorOptions>? playerClientOptions = null) 
            where TClansClient : ClansClientBase
            where TPlayersClient : PlayersClientBase
        {
            builder.ConfigureServices((context, services) => AddCocApiCache<TClansClient, TPlayersClient>(services, provider, clanClientOptions, playerClientOptions));

            return builder;
        }
    }
}
