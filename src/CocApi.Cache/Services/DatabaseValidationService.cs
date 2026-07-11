using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CocApi.Cache.Services;

internal sealed class DatabaseValidationService : IHostedService
{
    private readonly ILogger<DatabaseValidationService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseValidationService(
        ILogger<DatabaseValidationService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();
            await dbContext.Clans.Select(c => c.Id).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Failed to query the database. You may need to run a migration.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
