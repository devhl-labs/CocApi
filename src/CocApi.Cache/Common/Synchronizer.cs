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

    public Synchronizer(ILogger<Synchronizer> logger, IOptions<CacheOptions> options)
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        int max = options.Value.MaxConcurrency;
        UpdateSemaphore = new SemaphoreSlim(max, max);
    }

    public void Dispose()
    {
        ClanLock.Dispose();
        VillageLock.Dispose();
        WarLock.Dispose();
        CwlWarLock.Dispose();
        UpdateSemaphore.Dispose();
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
}
