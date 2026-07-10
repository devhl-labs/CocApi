using System;
using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options;

public class NewCwlWarServiceOptions : RecurringServiceOptions
{
    public NewCwlWarServiceOptions()
    {
        DelayBeforeExecution = TimeSpan.FromSeconds(5);
        DelayBetweenExecutions = TimeSpan.FromMinutes(2);
        Enabled = true;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int ConcurrentUpdates { get; set; } = 10;
}
