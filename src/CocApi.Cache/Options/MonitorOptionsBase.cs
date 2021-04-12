using System;

namespace CocApi.Cache
{
    public class MonitorOptionsBase
    {
        /// <summary>
        /// Default value is 250 milliseconds.
        /// </summary>
        public TimeSpan DelayBetweenBatchUpdates { get; set; } = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Default value is 250 milliseconds.
        /// </summary>
        public TimeSpan DelayBetweenBatches { get; set; } = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Default value is true.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
