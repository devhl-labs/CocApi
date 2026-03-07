using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Cache;

internal sealed class TagLock : IDisposable
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();
    private readonly System.Timers.Timer _timer;

    internal TagLock(TimeSpan? cleanupInterval = null)
    {
        _timer = new System.Timers.Timer((cleanupInterval ?? TimeSpan.FromMinutes(5)).TotalMilliseconds);
        _timer.Elapsed += (_, _) => Cleanup();
        _timer.AutoReset = true;
        _timer.Start();
    }

    public bool TryAcquire(string tag)
    {
        var semaphore = _semaphores.GetOrAdd(tag, _ => new SemaphoreSlim(1, 1));
        return semaphore.Wait(0);
    }

    public Task AcquireAsync(string tag, CancellationToken cancellationToken = default)
    {
        var semaphore = _semaphores.GetOrAdd(tag, _ => new SemaphoreSlim(1, 1));
        return semaphore.WaitAsync(cancellationToken);
    }

    public void Release(string tag)
    {
        if (_semaphores.TryGetValue(tag, out var semaphore))
            semaphore.Release();
    }

    private void Cleanup()
    {
        foreach (var (tag, semaphore) in _semaphores)
        {
            if (!semaphore.Wait(0))
                continue;

            // Only remove if this exact instance is still in the dictionary.
            // If lost the race (another thread replaced it), just release.
            if (!_semaphores.TryRemove(new KeyValuePair<string, SemaphoreSlim>(tag, semaphore)))
                semaphore.Release();
            // Intentionally not disposing: SemaphoreSlim has no unmanaged resources
            // unless AvailableWaitHandle is accessed (which we never do).
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
        foreach (var semaphore in _semaphores.Values)
            semaphore.Dispose();
        _semaphores.Clear();
    }
}
