﻿using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading.Tasks;
using CocApi.Cache.Services;

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
        internal static async Task OnLog(object sender, LogEventArgs log)
        {
            try
            {
                if (Log != null)
                    await Log.Invoke(sender, log).ConfigureAwait(false);
            }
            catch (Exception)
            {
                //throw;
            }
        }

        internal static bool EnsureSingleton(bool instantiated)
        {
            if (instantiated)
                throw new InvalidOperationException("This singleton has already been instantiated.");

            return true;
        }

        public static event LogEventHandler? Log;

        private static long _currentSemaphoreUsage = 0;

        private static int _maxCount = 25;

        private static SemaphoreSlim _concurrentEventsSemaphore = new(_maxCount, _maxCount);

        internal static void SetMaxConcurrentEvents(int max)
        {
            _maxCount = max;
            _concurrentEventsSemaphore = new SemaphoreSlim(max, max);
        }

        internal static async Task SendConcurrentEvent(object sender, Func<Task> action, CancellationToken cancellationToken)
        {
            if (Interlocked.Read(ref _currentSemaphoreUsage) >= _maxCount)
                await OnLog(sender, new LogEventArgs(LogLevel.Trace, message: "Max concurrent events reached.")).ConfigureAwait(false);

            await _concurrentEventsSemaphore.WaitAsync(cancellationToken);

            try
            {
                Interlocked.Increment(ref _currentSemaphoreUsage);

                await action().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await OnLog(sender, new LogEventArgs(LogLevel.Error, e /*, "Error on clan updated."*/)); // TODO: fix this error message
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
            services.AddSingleton<DeleteStalePlayerService>();
        }





        public static void AddCocApiCache(
            this IServiceCollection services,
            Action<CacheDbContextFactoryProvider> provider,
            Action<CacheOptions>? cacheOptions = null)
            => AddCocApiCache<ClansClient, PlayersClient, TimeToLiveProvider>(
                services, provider, cacheOptions);

        public static void AddCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(
            this IServiceCollection services, 
            Action<CacheDbContextFactoryProvider> provider,
            Action<CacheOptions>? cacheOptions = null) 
            where TClansClient : ClansClient
            where TPlayersClient : PlayersClient
            where TTimeToLiveProvider : TimeToLiveProvider
        {
            if (!services.Any(x => x.ServiceType == typeof(CocApi.Api.ClansApi)))
                throw new InvalidOperationException("ClansApi was not found in the service collection. Add it using AddCocApi");

            if (!services.Any(x => x.ServiceType == typeof(CocApi.Api.PlayersApi)))
                throw new InvalidOperationException("PlayersApi was not found in the service collection. Add it using AddCocApi");

            if (provider == null)
                throw new InvalidOperationException("The DbContext provider was null.");

            if (cacheOptions != null)
                services.Configure(cacheOptions);

            services.AddSingletons<TTimeToLiveProvider>();

            services.AddCocApiDbContext(provider);

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
            services.AddHostedService(services => services.GetRequiredService<DeleteStalePlayerService>());
        }

        public static IHostBuilder ConfigureCocApiCache(
            this IHostBuilder builder,
            Action<CacheDbContextFactoryProvider> provider,
            Action<CacheOptions>? cacheOptions = null)
            => ConfigureCocApiCache<ClansClient, PlayersClient, TimeToLiveProvider>(
                builder, provider, cacheOptions);

        public static IHostBuilder ConfigureCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(
            this IHostBuilder builder,
            Action<CacheDbContextFactoryProvider> provider,
            Action<CacheOptions>? cacheOptions = null) 
            where TClansClient : ClansClient
            where TPlayersClient : PlayersClient
            where TTimeToLiveProvider : TimeToLiveProvider
        {
            builder.ConfigureServices((context, services) =>
                AddCocApiCache<TClansClient, TPlayersClient, TTimeToLiveProvider>(
                    services, provider, cacheOptions));

            return builder;
        }
    }
}
