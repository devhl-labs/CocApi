using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public static class Library
    {
        private static IServiceProvider? _services;
        private static readonly object _serviceProviderLock = new object();

        internal static IServiceProvider BuildServiceProvider(string connectionString)
        {
            lock (_serviceProviderLock)
            {
                if (_services != null)
                    return _services;

                var services = new ServiceCollection()
                    .AddDbContext<CacheContext>(o =>
                        o.UseSqlite(connectionString))
                    .BuildServiceProvider();

                _services = services;

                return _services;
            }
        }

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

        public static int MaxConcurrentEvents { get; set; } = 50;

        internal static SemaphoreSlim ConcurrentEventsSemaphore = new SemaphoreSlim(MaxConcurrentEvents, MaxConcurrentEvents);

        public static MonitorOptions ClanMonitorOptions { get; } = new MonitorOptions();
        public static MonitorOptions MemberMonitorOptions { get; } = new MonitorOptions();
        public static MonitorOptions NewCwlWarMonitorOptions { get; } = new MonitorOptions { DelayBetweenTasks = TimeSpan.FromMinutes(2), ConcurrentUpdates = 10 };
        public static MonitorOptions NewWarMonitorOptions { get; } = new MonitorOptions { DelayBetweenTasks = TimeSpan.FromSeconds(15) };
        public static MonitorOptions PlayerMonitorOptions { get; } = new MonitorOptions { DelayBetweenTasks = TimeSpan.FromMinutes(2) };
        public static MonitorOptions UnmonitoredClansMonitorOptions { get; } = new MonitorOptions { DelayBetweenTasks = TimeSpan.FromMinutes(2) };
        public static MonitorOptions WarMonitorOptions { get; } = new MonitorOptions();

        public static class TableNames
        {
            public static string Clans { get; set; } = "clan";

            public static string CurrentWar { get; set; } = "current_war";

            public static string WarLog { get; set; } = "war_log";

            public static string Group { get; set; } = "group";

            public static string Player { get; set; } = "player";

            public static string War { get; set; } = "war";
        }
    }
}
