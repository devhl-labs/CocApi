using CocApi.Cache.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Test
{
    public class DownloaderServiceTest : BackgroundService
    {
        public DownloaderServiceTest(ILogger<DownloaderService> logger, DownloaderService downloaderService)
        {
            Logger = logger;
            DownloaderService = downloaderService;
        }

        public ILogger<DownloaderService> Logger { get; }
        public DownloaderService DownloaderService { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            Logger.LogInformation("pausing downloading");

            await DownloaderService.StopAsync();

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            Logger.LogInformation("resuming downloading");

            await DownloaderService.StartAsync();
        }
    }
}
