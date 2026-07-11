using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScheduledServices;

namespace CocApi.Cache.Services;

public abstract class ServiceBase<TRecurringOptions> : RecurringService<TRecurringOptions>
    where TRecurringOptions : class, IRecurringServiceOptions
{
    private const long _slowCycleThresholdMs = 5000;

    protected readonly record struct CycleCounters(
        int Fetched,
        int Updated,
        int LockSkips,
        long SaveMs);

    private protected int _id = int.MinValue;
    private protected DateTime expires = DateTime.UtcNow.AddSeconds(-3);
    private protected DateTime min = DateTime.MinValue;
    private protected DateTime now = DateTime.UtcNow;

    private readonly ILogger _lifecycleLogger;

    private protected IServiceScopeFactory ScopeFactory { get; }


    public ServiceBase(
        ILogger logger,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TRecurringOptions> options,
        ILoggerFactory loggerFactory
        ) : base(loggerFactory, options)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        _lifecycleLogger = loggerFactory.CreateLogger("CocApi.Cache.Services.ServiceBase");
        ScopeFactory = scopeFactory;
    }

    protected sealed override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        var cycleSw = System.Diagnostics.Stopwatch.StartNew();
        LogCycleStarted();
        SetDateVariables();

        CycleCounters counters = await ExecuteCycleAsync(cancellationToken).ConfigureAwait(false);

        cycleSw.Stop();
        LogCycleEnded(counters, cycleSw.ElapsedMilliseconds);

        if (cycleSw.ElapsedMilliseconds > _slowCycleThresholdMs)
        {
            LogCycleSlow(counters, cycleSw.ElapsedMilliseconds);
        }
    }

    protected abstract Task<CycleCounters> ExecuteCycleAsync(CancellationToken cancellationToken);

    private protected void LogCycleStarted()
    {
        _lifecycleLogger.LogTrace(CacheLogEvents.ServiceCycleStarted, "{ServiceName} cycle started", GetType().Name);
    }

    private protected void LogCycleEnded(CycleCounters counters, long totalMs)
    {
        _lifecycleLogger.LogTrace(
            CacheLogEvents.ServiceCycleEnded,
            "{ServiceName} cycle ended | Fetched={Fetched} | Updated={Updated} | LockSkips={LockSkips} | SaveMs={SaveMs} | TotalMs={TotalMs}",
            GetType().Name, counters.Fetched, counters.Updated, counters.LockSkips, counters.SaveMs, totalMs);
    }

    private void LogCycleSlow(CycleCounters counters, long totalMs)
    {
        _lifecycleLogger.LogDebug(
            CacheLogEvents.ServiceCycleSlow,
            "{ServiceName} cycle slow | Fetched={Fetched} | Updated={Updated} | LockSkips={LockSkips} | SaveMs={SaveMs} | TotalMs={TotalMs}",
            GetType().Name, counters.Fetched, counters.Updated, counters.LockSkips, counters.SaveMs, totalMs);
    }


    private protected void SetDateVariables()
    {
        expires = DateTime.UtcNow.AddSeconds(-3);

        now = DateTime.UtcNow;
    }
}
