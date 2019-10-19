using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using CocApiLibrary;
using System.Threading.Tasks;

namespace CocApiConsoleTest
{
    class Program
    {
        private static CocApi? _cocApi = null;

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            LogService logService = host.Services.GetRequiredService<LogService>();

            logService.LogInformation("Press CTRL-C to exit");

            Console.CancelKeyPress += (s, e) => DoExitStuff(host.Services);

            InitializeCocApi(host.Services);

            host.Services.GetRequiredService<EventHandlerService>();

            host.Run();

            await Task.Delay(1);
        }     

        public static IHostBuilder CreateHostBuilder(string[] args) =>

            Host.CreateDefaultBuilder(args)

            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<LogService>();

                services.AddSingleton<CocApi>();

                services.AddSingleton<EventHandlerService>();
            })
        ;

        private static void InitializeCocApi(IServiceProvider serviceProvider)
        {
            CocApiConfiguration cocApiConfiguration = new CocApiConfiguration
            {
                NumberOfUpdaters = 1,

                TimeToWaitForWebRequests = new TimeSpan(0, 0, 0, 10, 0),
            };

            cocApiConfiguration.Tokens.Add(File.ReadAllText(@"E:\Desktop\token.txt"));

            _cocApi = serviceProvider.GetRequiredService<CocApi>();

            _cocApi.Initialize(cocApiConfiguration, serviceProvider.GetRequiredService<LogService>());

            _cocApi.DownloadLeagueWars = DownloadLeagueWars.Auto;

            _cocApi.DownloadVillages = false;            
            
            List<string> clans = new List<string>
            {
                "#8J82PV0C",  // FYSB Unbuckled
                "#2C8V29YJ",  // Зеленоград
                "#22VCPLR98", // LostMeta Power
                "#8RJJ0C0Y",   // Rising Asylum
                "#22G0JJR8",   // FYSB
                "#P989QU9P",   // Burlap Thongs
            };

            _cocApi.WatchClans(clans);

            _cocApi.BeginUpdatingClans();

            return;
        }

        private static void DoExitStuff(IServiceProvider services)
        {
            services.GetRequiredService<LogService>().LogInformation("{program}: Quiting, please wait...", "Program.cs");

            _cocApi?.Dispose();
        }

    }
}