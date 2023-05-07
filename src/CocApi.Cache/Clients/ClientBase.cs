using System;
using System.Linq;
using CocApi.Cache.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Cache;

public class ClientBase<T>
{
    internal Synchronizer Synchronizer { get; }
    public ILogger<T> Logger { get; }
    public IServiceScopeFactory ScopeFactory { get; }

    public ClientBase(
        ILogger<T> logger,
        IServiceScopeFactory scopeFactory,
        Synchronizer synchronizer,
        IOptions<CacheOptions> options)
    {
        Library.SetMaxConcurrentEvents(options.Value.MaxConcurrentEvents);
        Logger = logger;
        ScopeFactory = scopeFactory;
        EnsureMigrated();
        Synchronizer = synchronizer;
    }

    private void EnsureMigrated()
    {
        try
        {
            using var scope = ScopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

            dbContext.Clans.OrderBy(c => c.Id).FirstOrDefault();
        }
        catch (Exception e)
        {
            throw new Exception("Failed to query the database. You may need to run a migration.", e);
        }
    }
}
