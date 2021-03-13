using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        public static int MaxConcurrentEvents { get; set; } = 25;

        internal static SemaphoreSlim ConcurrentEventsSemaphore = new(MaxConcurrentEvents, MaxConcurrentEvents);

        public static class TableNames
        {
            public static string Clans { get; set; } = "clan";

            public static string CurrentWar { get; set; } = "current_war";

            public static string WarLog { get; set; } = "war_log";

            public static string Group { get; set; } = "group";

            public static string Player { get; set; } = "player";

            public static string War { get; set; } = "war";
        }

        public static void AddCocApiDbContext(this IServiceCollection services, Action<CacheDbContextFactoryProvider> configure)
        {
            CacheDbContextFactoryProvider provider = new();

            configure(provider);

            if (provider.Factory == null)
                throw new ArgumentNullException(nameof(provider.Factory), "DbContextFactory cannot be null. This object is used to connect to your database.");

            services.AddSingleton(provider);
        }

        public static IHostBuilder ConfigureCocApiDbContext(this IHostBuilder builder, Action<CacheDbContextFactoryProvider> provider)
        {
            builder.ConfigureServices((context, services) => AddCocApiDbContext(services, provider));

            return builder;
        }

        public static void AddPlayersClient(this IServiceCollection services, Action<MonitorOptions>? options = null)
            => AddPlayersClient<PlayersClientBase>(services, options);

        public static void AddPlayersClient<TPlayersClient>(this IServiceCollection services, Action<MonitorOptions>? options = null) where TPlayersClient : PlayersClientBase
        {
            if (options != null)
                services.AddOptions<MonitorOptions>().Configure(options);

            services.AddSingleton<PlayersClientBase, TPlayersClient>();
        }

        public static IHostBuilder ConfigurePlayersClient(this IHostBuilder builder, Action<MonitorOptions>? options = null)
            => ConfigurePlayersClient<PlayersClientBase>(builder, options);

        public static IHostBuilder ConfigurePlayersClient<TPlayersClient>(this IHostBuilder builder, Action<MonitorOptions>? options = null)
        where TPlayersClient : PlayersClientBase
        {
            builder.ConfigureServices((context, services) => AddPlayersClient<TPlayersClient>(services, options));

            return builder;
        }

        public static void AddClansClient(this IServiceCollection services, Action<ClanMonitorsOptions>? options = null)
            => AddClansClient<ClansClientBase>(services, options);

        public static void AddClansClient<TClansClient>(this IServiceCollection services, Action<ClanMonitorsOptions>? options = null) where TClansClient : ClansClientBase
        {
            services.AddSingleton<TClansClient>();

            services.AddSingleton<ClansClientBase, TClansClient>();

            services.TryAddSingleton<PlayersClientBase>();

            services.AddHostedService((serviceProvider) =>
            {
                ClansClientBase clansClient = serviceProvider.GetRequiredService<TClansClient>();

                if (options != null)
                    services.AddOptions<ClanMonitorsOptions>().Configure(options);

                return clansClient;
            });
        }

        public static IHostBuilder ConfigureClansClient(this IHostBuilder builder, Action<ClanMonitorsOptions>? options = null)
            => ConfigureClansClient<ClansClientBase>(builder, options);

        public static IHostBuilder ConfigureClansClient<TClansClient>(this IHostBuilder builder, Action<ClanMonitorsOptions>? options = null) 
            where TClansClient : ClansClientBase
        {
            builder.ConfigureServices((context, services) => AddClansClient<TClansClient>(services, options));

            return builder;
        }
    }
}
