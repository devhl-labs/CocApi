using System;

namespace CocApi.Cache
{
    public class MonitorOptionsBase
    {
        public TimeSpan DelayBetweenBatchUpdates { get; set; } = TimeSpan.FromMilliseconds(250);

        public TimeSpan DelayBetweenBatches { get; set; } = TimeSpan.FromMilliseconds(250);

        public int ConcurrentUpdates { get; set; } = 50;

        public bool IsDisabled { get; set; }
    }

    public class MonitorOptions : MonitorOptionsBase
    {
        //public TimeSpan DelayBetweenBatchUpdates { get; set; } = TimeSpan.FromMilliseconds(250);

        //public TimeSpan DelayBetweenBatches { get; set; } = TimeSpan.FromMilliseconds(250);

        public int ConcurrentUpdates { get; set; } = 50;

        //public bool IsDisabled { get; set; }
    }
}
