using System;
using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options;

public class NewWarServiceOptions : RecurringServiceOptions
{
    public NewWarServiceOptions()
    {
        DelayBeforeExecution = TimeSpan.FromSeconds(5);
        DelayBetweenExecutions = TimeSpan.FromSeconds(15);
        Enabled = true;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int ConcurrentUpdates { get; set; } = 50;
}
