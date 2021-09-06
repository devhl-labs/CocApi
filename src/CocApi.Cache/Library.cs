using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace CocApi.Cache
{
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

        private static long _currentSemaphoreUsage = 0;

        private static int _maxCount = 25;

        private static SemaphoreSlim _concurrentEventsSemaphore = new(_maxCount, _maxCount);

        public static void SetMaxConcurrentEvents(int max)
        {
            _maxCount = max;
            _concurrentEventsSemaphore = new SemaphoreSlim(max, max);
        }

        internal static async Task SendConcurrentEvent(object sender, Action action, CancellationToken cancellationToken)
        {
            if (Interlocked.Read(ref _currentSemaphoreUsage) >= _maxCount)
                OnLog(sender, new LogEventArgs(LogLevel.Trace, message: "Max concurrent events reached."));

            await _concurrentEventsSemaphore.WaitAsync(cancellationToken);

            try
            {
                Interlocked.Increment(ref _currentSemaphoreUsage);

                action();
            }
            catch (Exception e)
            {
                OnLog(sender, new LogEventArgs(LogLevel.Error, e /*, "Error on clan updated."*/)); // TODO: fix this error message
            }
            finally
            {
                _concurrentEventsSemaphore.Release();

                Interlocked.Decrement(ref _currentSemaphoreUsage);
            }
        }

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







        public static void AddCocApiCache(
            this IServiceCollection services,
            Action<CacheDbContextFactoryProvider> provider,
            int maxConcurrentEvents = 25,
            Action<ClanClientOptions>? clanClientOptions = null, 
            Action<MonitorOptions>? playerClientOptions = null)
            => AddCocApiCache<ClansClientBase, PlayersClientBase>(services, provider, clanClientOptions, playerClientOptions, maxConcurrentEvents);

        public static void AddCocApiCache<TClansClient, TPlayersClient>(
            this IServiceCollection services, 
            Action<CacheDbContextFactoryProvider> provider,
            Action<ClanClientOptions>? clanClientOptions = null,
            Action<MonitorOptions>? playerClientOptions = null,
            int maxConcurrentEvents = 25) 
            where TClansClient : ClansClientBase
            where TPlayersClient : PlayersClientBase
        {
            if (!services.Any(x => x.ServiceType == typeof(CocApi.Api.ClansApi)) ||
                !services.Any(x => x.ServiceType == typeof(CocApi.Api.PlayersApi)))
                throw new InvalidOperationException("ClansApi or PlayersApi were not found in the service collection.");

            if (provider == null)
                throw new InvalidOperationException("The DbContext provider was null.");

            SetMaxConcurrentEvents(maxConcurrentEvents);

            services.AddCocApiDbContext(provider);

            services.AddPlayersClient<TPlayersClient>(playerClientOptions);

            services.AddClansClient<TClansClient>(clanClientOptions);
        }

        public static IHostBuilder ConfigureCocApiCache(
            this IHostBuilder builder,
            Action<CacheDbContextFactoryProvider> provider,
            Action<ClanClientOptions>? clanClientOptions = null,
            Action<MonitorOptions>? playerClientOptions = null,
            int maxConcurrentEvents = 25)
            => ConfigureCocApiCache<ClansClientBase, PlayersClientBase>(builder, provider, clanClientOptions, playerClientOptions, maxConcurrentEvents);

        public static IHostBuilder ConfigureCocApiCache<TClansClient, TPlayersClient>(
            this IHostBuilder builder,
            Action<CacheDbContextFactoryProvider> provider,
            Action<ClanClientOptions>? clanClientOptions = null,
            Action<MonitorOptions>? playerClientOptions = null,
            int maxConcurrentEvents = 25) 
            where TClansClient : ClansClientBase
            where TPlayersClient : PlayersClientBase
        {
            builder.ConfigureServices((context, services) => 
                AddCocApiCache<TClansClient, TPlayersClient>(services, provider, clanClientOptions, playerClientOptions, maxConcurrentEvents));

            return builder;
        }
    }
}
