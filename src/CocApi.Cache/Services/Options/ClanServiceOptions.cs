using System;
using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options;

public class ClanServiceOptions : RecurringServiceOptions
{
    public ClanServiceOptions()
    {
        DelayBeforeExecution = TimeSpan.FromSeconds(5);
        DelayBetweenExecutions = TimeSpan.FromSeconds(5);
        Enabled = true;
    }

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
    public bool DownloadWarLog { get; set; } = true;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int ConcurrentUpdates { get; set; } = 200;
}
