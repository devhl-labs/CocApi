using System;

namespace CocApi.Cache
{
    public class MonitorOptions
    {
        public TimeSpan DelayBetweenTasks { get; set; } = TimeSpan.FromMilliseconds(250);

        public int ConcurrentUpdates { get; set; } = 100;

        public bool IsDisabled { get; set; }
    }
}
