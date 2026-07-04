using CocApi.Cache.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScheduledServices;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Cache.Services;

public sealed class ThreadPoolMonitorService : RecurringService
{
    private readonly ILogger<ThreadPoolMonitorService> _logger;
    private readonly IOptionsMonitor<CacheOptions> _cacheOptions;
    private int _pressureStreak;

    internal static bool Instantiated { get; private set; }

    public ThreadPoolMonitorService(
        ILogger<ThreadPoolMonitorService> logger,
        IOptionsMonitor<CacheOptions> cacheOptions)
        : base(logger, Microsoft.Extensions.Options.Options.Create(cacheOptions.CurrentValue.ThreadPoolMonitor))
    {
        _logger = logger;
        _cacheOptions = cacheOptions;
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
    }

    protected override Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        ThreadPoolMonitorServiceOptions options = _cacheOptions.CurrentValue.ThreadPoolMonitor;

        ThreadPool.GetMinThreads(out int minWorker, out int minIocp);
        ThreadPool.GetMaxThreads(out int maxWorker, out int maxIocp);
        ThreadPool.GetAvailableThreads(out int availWorker, out int availIocp);

        int usedWorker = maxWorker - availWorker;
        int usedIocp = maxIocp - availIocp;
        int threadCount = ThreadPool.ThreadCount;
        long pendingItems = ThreadPool.PendingWorkItemCount;

        bool underPressure =
            availWorker <= options.MinAvailableWorkerThreads ||
            pendingItems >= options.PendingWorkItemWarningThreshold;

        if (underPressure)
        {
            _pressureStreak++;

            if (_pressureStreak >= options.ConsecutivePressureCyclesForWarning)
            {
                _logger.LogWarning(
                    "ThreadPool monitor pressure | Streak={Streak} | Worker={UsedWorker}/{MinWorker}/{MaxWorker} avail={AvailWorker} | IOCP={UsedIocp}/{MinIocp}/{MaxIocp} avail={AvailIocp} | Pending={Pending} | Threads={ThreadCount}",
                    _pressureStreak,
                    usedWorker, minWorker, maxWorker, availWorker,
                    usedIocp, minIocp, maxIocp, availIocp,
                    pendingItems, threadCount);
            }
            else if (options.LogSnapshotEachCycle)
            {
                _logger.LogDebug(
                    "ThreadPool monitor transient pressure | Streak={Streak}/{Required} | Worker={UsedWorker}/{MinWorker}/{MaxWorker} avail={AvailWorker} | IOCP={UsedIocp}/{MinIocp}/{MaxIocp} avail={AvailIocp} | Pending={Pending} | Threads={ThreadCount}",
                    _pressureStreak, options.ConsecutivePressureCyclesForWarning,
                    usedWorker, minWorker, maxWorker, availWorker,
                    usedIocp, minIocp, maxIocp, availIocp,
                    pendingItems, threadCount);
            }
        }
        else
        {
            if (_pressureStreak >= options.ConsecutivePressureCyclesForWarning)
            {
                _logger.LogInformation(
                    "ThreadPool monitor recovered | PreviousStreak={Streak} | Worker={UsedWorker}/{MinWorker}/{MaxWorker} avail={AvailWorker} | IOCP={UsedIocp}/{MinIocp}/{MaxIocp} avail={AvailIocp} | Pending={Pending} | Threads={ThreadCount}",
                    _pressureStreak,
                    usedWorker, minWorker, maxWorker, availWorker,
                    usedIocp, minIocp, maxIocp, availIocp,
                    pendingItems, threadCount);
            }
            else if (options.LogSnapshotEachCycle)
            {
                _logger.LogTrace(
                    "ThreadPool monitor healthy | Worker={UsedWorker}/{MinWorker}/{MaxWorker} avail={AvailWorker} | IOCP={UsedIocp}/{MinIocp}/{MaxIocp} avail={AvailIocp} | Pending={Pending} | Threads={ThreadCount}",
                    usedWorker, minWorker, maxWorker, availWorker,
                    usedIocp, minIocp, maxIocp, availIocp,
                    pendingItems, threadCount);
            }

            _pressureStreak = 0;
        }

        return Task.CompletedTask;
    }
}
