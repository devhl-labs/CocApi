using System;
using System.Threading;

namespace CocApi.Cache
{
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

        internal static SemaphoreSlim ConcurrentEventsSemaphore = new SemaphoreSlim(MaxConcurrentEvents, MaxConcurrentEvents);

        public static class Monitors
        {
            public static MonitorOptions Clans { get; } = new MonitorOptions();
            public static MonitorOptionsBase Members { get; } = new MonitorOptionsBase();
            public static MonitorOptions NewCwlWars { get; } = new MonitorOptions { DelayBetweenBatches = TimeSpan.FromMinutes(2), DelayBetweenBatchUpdates = TimeSpan.FromMinutes(2), ConcurrentUpdates = 10 };
            public static MonitorOptions NewWars { get; } = new MonitorOptions { DelayBetweenBatches = TimeSpan.FromSeconds(15), DelayBetweenBatchUpdates = TimeSpan.FromSeconds(15) };
            public static MonitorOptions Players { get; } = new MonitorOptions();
            public static MonitorOptions ActiveWars { get; } = new MonitorOptions { DelayBetweenBatches = TimeSpan.FromMinutes(2), DelayBetweenBatchUpdates = TimeSpan.FromMinutes(2) };
            public static MonitorOptions Wars { get; } = new MonitorOptions();
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
    }
}
