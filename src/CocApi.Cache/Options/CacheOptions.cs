using System;
using System.ComponentModel;

namespace CocApi.Cache
{
    public class CacheOptions
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int MaxConcurrentEvents { get; set; } = 25;
        public MonitorOptions ActiveWars { get; } = new MonitorOptions { DelayBeforeExecution = TimeSpan.FromMinutes(2), DelayBetweenExecutions = TimeSpan.FromMinutes(2) };





        public ClanMonitorOptions Clans { get; } = new ClanMonitorOptions { ConcurrentUpdates = 500 };
        
        public MonitorOptionsBase ClanMembers { get; } = new MonitorOptionsBase();
        
        public MonitorOptions NewCwlWars { get; } = new MonitorOptions { DelayBetweenExecutions = TimeSpan.FromMinutes(2), ConcurrentUpdates = 10 };
        
        public MonitorOptions NewWars { get; } = new MonitorOptions { DelayBetweenExecutions = TimeSpan.FromSeconds(15), ConcurrentUpdates =  100 };
        

        public MonitorOptions Wars { get; } = new MonitorOptions();
        
        public MonitorOptions CwlWars { get; } = new MonitorOptions();
    }
}
