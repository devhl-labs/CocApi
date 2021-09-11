using System;
using System.ComponentModel;

namespace CocApi.Cache
{
    public class ServiceOptionsBase
    {
        /// <summary>
        /// Default value is true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TimeSpan DelayBeforeExecution { get; set; } = TimeSpan.Zero;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TimeSpan DelayBetweenExecutions { get; set; } = TimeSpan.Zero;
    }
}
