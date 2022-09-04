using CocApi.Cache.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Test;

public class CachingServiceTest : BackgroundService
{
    public CachingServiceTest(ILogger<CachingService> logger, CachingService cachingService)
    {
        Logger = logger;
        CachingService = cachingService;
    }

    public ILogger<CachingService> Logger { get; }
    public CachingService CachingService { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        Logger.LogInformation("pausing downloading");

        await CachingService.StopAsync();

        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        Logger.LogInformation("resuming downloading");

        await CachingService.StartAsync();
    }
}
