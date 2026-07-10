using System;
using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options;

public class ClanWarServiceOptions : RecurringServiceOptions
{
    public ClanWarServiceOptions()
    {
        DelayBeforeExecution = TimeSpan.FromSeconds(15);
        DelayBetweenExecutions = TimeSpan.FromSeconds(5);
        Enabled = true;
    }

    /// <summary>
    /// Default value is true.
    /// </summary>
    public bool DownloadCurrentWar { get; set; } = true;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int ConcurrentUpdates { get; set; } = 200;
}
