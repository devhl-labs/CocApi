using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CocApi.Rest.Client;

namespace CocApi.Cache
{
    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;


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
        public static JsonSerializerOptions? JsonSerializerOptions { get; set; }

        internal static bool WarnOnSubsequentInstantiations<T>(ILogger<T> logger, bool instantiated)
        {
            if (instantiated)
                logger.LogWarning("{typeName} is intended to be a singleton but was instantiated more than once.", typeof(T).Name);

            return true;
        }

        private static long _currentSemaphoreUsage = 0;

        private static int _maxCount = 25;

        private static SemaphoreSlim _concurrentEventsSemaphore = new(_maxCount, _maxCount);

        internal static void SetMaxConcurrentEvents(int max)
        {
            _maxCount = max;
            _concurrentEventsSemaphore = new SemaphoreSlim(max, max);
        }

        internal static async Task SendConcurrentEvent<T>(ILogger<T> logger, string methodName, Func<Task> action, CancellationToken cancellationToken)
        {
            if (Interlocked.Read(ref _currentSemaphoreUsage) >= _maxCount)
                logger.LogWarning("Max concurrent events reached.");

            await _concurrentEventsSemaphore.WaitAsync(cancellationToken);

            try
            {
                Interlocked.Increment(ref _currentSemaphoreUsage);

                await action().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", typeof(T).Name, methodName);
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

        // this should be refactored out
        internal static void AddStaticJsonOptions(IServiceCollection services)
        {
            ServiceDescriptor jsonSerializerServiceDescriptor = services.First(s => s.ServiceType == typeof(JsonSerializerOptionsProvider));

            JsonSerializerOptionsProvider jsonOptions = (JsonSerializerOptionsProvider)jsonSerializerServiceDescriptor.ImplementationInstance!;

            JsonSerializerOptions = jsonOptions.Options;
        }
    }
}
