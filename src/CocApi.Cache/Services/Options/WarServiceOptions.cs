using System;
using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options;

public class WarServiceOptions : RecurringServiceOptions
{
    public WarServiceOptions()
    {
        DelayBeforeExecution = TimeSpan.FromSeconds(25);
        DelayBetweenExecutions = TimeSpan.FromSeconds(5);
        Enabled = true;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int ConcurrentUpdates { get; set; } = 500;
}
