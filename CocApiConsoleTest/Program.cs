using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using devhl.CocApi;

namespace CocApiConsoleTest
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var services = ConfigureServices();

            Console.CancelKeyPress += (s, e) => DoExitStuff(services);

            LogService logService = services.GetRequiredService<LogService>();

            logService.Log(LogLevel.Information, nameof(Program), null, "Press CTRL-C to exit");

            ConfigureCocApi(services);

            services.GetRequiredService<EventHandlerService>();

            await Task.Delay(-1);

            if (args == null) { }
        }

        public static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<LogService>()
                .AddSingleton<CocApi>()
                .AddSingleton(GetCocApiConfiguration)
                .AddSingleton<EventHandlerService>()
                .BuildServiceProvider();
        }

        private static void ConfigureCocApi(IServiceProvider services)
        {
            CocApi cocApi = services.GetRequiredService<CocApi>();

            List<string> clans = new List<string>
            {
                "#8J82PV0C",   // FYSB Unbuckled
                "#2C8V29YJ",   // Зеленоград
                "#22VCPLR98",  // LostMeta Power
                "#8RJJ0C0Y",   // Rising Asylum
                "#22G0JJR8",   // FYSB
                "#P989QU9P",   // Burlap Thongs
                "#YJC98R20",   // The hunters
                "#GGR2GU8Y",   // kerala kings 2
                "#R09C9RR0",   // Parathanon
            };

            cocApi.Clans.Queue(clans); //optionally use the ClanBuilder object and Build method to insert Clan object loaded from your database
                                       //events will fire as if your program was never restarted

            cocApi.Clans.QueueClanVillages = true; //events in cocApi.Villages will fire on ClanVillages in clans that are in the clan queue

            cocApi.Wars.DownloadLeagueWars = DownloadLeagueWars.Auto; //league wars will download at the beginning of the month only

            //cocApi.Villages.Queue("#00000"); //you may watch individual villages as well. this queue may take a long time if there are many villages

            cocApi.Clans.StartQueue();

            cocApi.Wars.StartQueue();
        }

        private static CocApiConfiguration GetCocApiConfiguration(IServiceProvider serviceProvider)
        {
            CocApiConfiguration cocApiConfiguration = new CocApiConfiguration
            {
                //Do not hard code these values
                //Store them in a json file instead.
                
                NumberOfUpdaters = 1,  //list of clans to be updated can be split across multiple tasks to process faster
                TimeToWaitForWebRequests = TimeSpan.FromSeconds(5), //how long to wait on an HTTP response before canceling the task
                TokenTimeOut = TimeSpan.FromSeconds(1), //how long to wait before reusing a token, helps us avoid a rate limit from sc

                //minimum time to consider an object valid
                //if you want updates as fast as sc will provide them
                //do not provide a value for time to live
                LeagueGroupTimeToLive = TimeSpan.FromHours(1),
                LeagueGroupNotFoundTimeToLive = TimeSpan.FromHours(1),
                VillageTimeToLive = TimeSpan.FromHours(1)
            };

            cocApiConfiguration.Tokens.Add(File.ReadAllText(@"E:\Desktop\token.txt"));

            return cocApiConfiguration;
        }

        private static void DoExitStuff(IServiceProvider services)
        {
            services.GetRequiredService<LogService>().Log(LogLevel.Information, nameof(Program), null, "Quiting, please wait...");

            CocApi cocApi = services.GetRequiredService<CocApi>();

            cocApi.Clans.StopQueue();

            cocApi.Wars.StopQueue();

            cocApi.Villages.StopQueue();

            cocApi.Dispose();

            Environment.Exit(0);
        }
    }
}