using System;
using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options;

public class PlayerServiceOptions : RecurringServiceOptions
{
    public PlayerServiceOptions()
    {
        DelayBeforeExecution = TimeSpan.FromSeconds(5);
        DelayBetweenExecutions = TimeSpan.FromSeconds(5);
        Enabled = true;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int ConcurrentUpdates { get; set; } = 50;
}
