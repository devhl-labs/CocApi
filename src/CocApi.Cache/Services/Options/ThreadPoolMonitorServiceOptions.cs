using ScheduledServices.Services.Options;

namespace CocApi.Cache.Services.Options;

public sealed class ThreadPoolMonitorServiceOptions : RecurringServiceOptions
{
    public int MinAvailableWorkerThreads { get; set; } = 10;

    public int PendingWorkItemWarningThreshold { get; set; } = 100;

    public int ConsecutivePressureCyclesForWarning { get; set; } = 3;

    public bool LogSnapshotEachCycle { get; set; } = false;
}
