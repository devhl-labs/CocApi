using System;
using System.Collections.Concurrent;
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
}
