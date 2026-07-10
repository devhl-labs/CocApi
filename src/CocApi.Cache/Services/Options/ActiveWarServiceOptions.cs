using System;
using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options;

public class ActiveWarServiceOptions : RecurringServiceOptions
{
    public ActiveWarServiceOptions()
    {
        DelayBeforeExecution = TimeSpan.FromMinutes(5);
        DelayBetweenExecutions = TimeSpan.FromMinutes(10);
        Enabled = true;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int ConcurrentUpdates { get; set; } = 50;
}
