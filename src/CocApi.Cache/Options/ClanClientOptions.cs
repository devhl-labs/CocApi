using System;

namespace CocApi.Cache
{
    public class ClanClientOptions
    {
        public int MaxConcurrentEvents { get; set; } = 25;
        public ClanMonitorOptions Clans { get; } = new ClanMonitorOptions();
        public MonitorOptionsBase ClanMembers { get; } = new MonitorOptionsBase();
        public MonitorOptions NewCwlWars { get; } = new MonitorOptions { DelayBetweenBatches = TimeSpan.FromMinutes(2), DelayBetweenBatchUpdates = TimeSpan.FromMinutes(2), ConcurrentUpdates = 10 };
        public MonitorOptions NewWars { get; } = new MonitorOptions { DelayBetweenBatches = TimeSpan.FromSeconds(15), DelayBetweenBatchUpdates = TimeSpan.FromSeconds(15) };
        public MonitorOptions ActiveWars { get; } = new MonitorOptions { DelayBetweenBatches = TimeSpan.FromMinutes(2), DelayBetweenBatchUpdates = TimeSpan.FromMinutes(2) };
        public MonitorOptions Wars { get; } = new MonitorOptions();
        public MonitorOptions CwlWars { get; } = new MonitorOptions();
    }
}
