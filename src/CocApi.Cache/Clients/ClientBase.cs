using CocApi.Cache.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CocApi.Cache;

public class ClientBase<T>
{
    internal Synchronizer Synchronizer { get; }
    public ILogger<T> Logger { get; }
    public IServiceScopeFactory ScopeFactory { get; }

    public ClientBase(
        ILogger<T> logger,
        IServiceScopeFactory scopeFactory,
        Synchronizer synchronizer)
    {
        Logger = logger;
        ScopeFactory = scopeFactory;
        Synchronizer = synchronizer;
    }
}
