using System;
using ScheduledServices.Services.Options;

namespace CocApi.Cache.Services.Options;

public sealed class ThreadPoolMonitorServiceOptions : RecurringServiceOptions
{
    public ThreadPoolMonitorServiceOptions()
    {
        DelayBeforeExecution = TimeSpan.FromSeconds(5);
        DelayBetweenExecutions = TimeSpan.FromSeconds(10);
        Enabled = false;
    }

    public int MinAvailableWorkerThreads { get; set; } = 10;

    public int PendingWorkItemWarningThreshold { get; set; } = 100;

    public int ConsecutivePressureCyclesForWarning { get; set; } = 3;

    public bool LogSnapshotEachCycle { get; set; } = false;
}
