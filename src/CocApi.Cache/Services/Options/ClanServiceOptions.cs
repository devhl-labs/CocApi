using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options
{
    public class ClanServiceOptions : RecurringServiceOptions
    {
        /// <summary>
        /// Default value is true.
        /// </summary>
        public bool DownloadClan { get; set; } = true;

        /// <summary>
        /// Default value is true.
        /// </summary>
        public bool DownloadGroup { get; set; } = true;

        /// <summary>
        /// Default value is true.
        /// </summary>
        public bool DownloadCurrentWar { get; set; } = true;

        /// <summary>
        /// Default value is true.
        /// </summary>
        public bool DownloadWarLog { get; set; } = true;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int ConcurrentUpdates { get; set; } = 50;
    }
}
