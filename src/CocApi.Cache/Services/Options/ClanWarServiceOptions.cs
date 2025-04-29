using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options;

public class ClanWarServiceOptions : RecurringServiceOptions
{
    /// <summary>
    /// Default value is true.
    /// </summary>
    public bool DownloadCurrentWar { get; set; } = true;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int ConcurrentUpdates { get; set; } = 50;
}
