using System;

namespace CocApi.Cache
{
    public class MonitorOptionsBase
    {
        public TimeSpan DelayBetweenBatchUpdates { get; set; } = TimeSpan.FromMilliseconds(250);

        public TimeSpan DelayBetweenBatches { get; set; } = TimeSpan.FromMilliseconds(250);

        public bool IsDisabled { get; set; }
    }
}
