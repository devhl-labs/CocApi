using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CocApi.Cache;

public sealed class Synchronizer : IDisposable
{
    public static bool Instantiated { get; private set; }

    internal TagLock ClanLock { get; } = new();
    internal TagLock VillageLock { get; } = new();
    internal TagLock WarLock { get; } = new();
    internal TagLock CwlWarLock { get; } = new();

    public Synchronizer(ILogger<Synchronizer> logger)
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
    }

    public void Dispose()
    {
        ClanLock.Dispose();
        VillageLock.Dispose();
        WarLock.Dispose();
        CwlWarLock.Dispose();
    }

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
}
