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
                /*
                 *  These times are rather conservative incase the key you provide is 
                 *  used elsewhere.  In production you can make these whatever you want
                 *  You will certainly want the TokenTimeOut to be much faster,
                 *  perhaps half a second.
                 *  
                 *  Strongly recommend you print some things to your ILogger
                 *  so you can get warned of preemptive rate limits and rate limits.
                 *  
                 *  Preemptive rate limits are okay but tell you that you maxing out your TokenTimeOut.
                 *  Rate Limits are bad!  They indicate SC is throttling your token.
                 *  Preemptive rate limits are not the same as rate limits.
                 *  
                 *  Preemptive rate limits come from the TokenObject.
                 *  Rate Limits are actual errors that come from WebResponse.
                 */

                CacheHttpResponses = true,
                ClanApiModelTimeToLive = TimeSpan.FromMinutes(10),
                CurrentWarApiModelTimeToLive = TimeSpan.FromSeconds(15),
                LeagueGroupApiModelTimeToLive = TimeSpan.FromHours(1),
                LeagueWarApiModelTimeToLive = TimeSpan.FromSeconds(15),
                TokenTimeOut = TimeSpan.FromSeconds(3),
                VillageApiModelTimeToLive = TimeSpan.FromHours(1),
                WarLogApiModelTimeToLive = TimeSpan.FromHours(1),
                NumberOfUpdaters = 1,
                TimeToWaitForWebRequests = TimeSpan.FromSeconds(10),                
            };

            cocApiConfiguration.Tokens.Add(File.ReadAllText(@"E:\Desktop\token.txt"));

            _cocApi = serviceProvider.GetRequiredService<CocApi>();

            _cocApi.Initialize(cocApiConfiguration, serviceProvider.GetRequiredService<LogService>());

            _cocApi.DownloadLeagueWars = DownloadLeagueWars.True;

            _cocApi.DownloadVillages = true;            
            
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