using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Cache;

public sealed class Synchronizer : IDisposable
{
    public static bool Instantiated { get; private set; }

    internal TagLock ClanLock { get; } = new();
    internal TagLock VillageLock { get; } = new();
    internal TagLock WarLock { get; } = new();
    internal TagLock CwlWarLock { get; } = new();
    internal SemaphoreSlim UpdateSemaphore { get; }
    internal SemaphoreSlim EventSemaphore { get; }
    internal SemaphoreSlim SaveSemaphore { get; } = new SemaphoreSlim(1, 1);

    public Synchronizer(ILogger<Synchronizer> logger, IOptions<CacheOptions> options)
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);

        int maxUpdates = options.Value.MaxConcurrentUpdates;
        UpdateSemaphore = new SemaphoreSlim(maxUpdates, maxUpdates);

        int maxEvents = options.Value.MaxConcurrentEvents;
        EventSemaphore = new SemaphoreSlim(maxEvents, maxEvents);
    }

    public void Dispose()
    {
        ClanLock.Dispose();
        VillageLock.Dispose();
        WarLock.Dispose();
        CwlWarLock.Dispose();
        UpdateSemaphore.Dispose();
        EventSemaphore.Dispose();
        SaveSemaphore.Dispose();
    }

    internal async Task WithSemaphoreAsync(Task task)
    {
        try {
            await task.ConfigureAwait(false);
        }
        finally
        {
            UpdateSemaphore.Release();
        }
    }

    internal async Task SendConcurrentEventAsync<T>(ILogger<T> logger, string methodName, Func<Task> action, CancellationToken cancellationToken)
    {
        await EventSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", typeof(T).Name, methodName);
        }
        finally
        {
            EventSemaphore.Release();
        }
    }
}
