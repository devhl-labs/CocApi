using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using CocApi.Cache;
using System.IO;
using CocApi.Model;
using System.Linq;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Web;
using System.Net;
using System.Text;
using CocApi.Client;

namespace CocApi.Test
{
    class Program
    {
        public static LogService LogService { get; set; }

        public static async Task Main(string[] args)
        {
            var services = ConfigureServices();

            LogService = services.GetRequiredService<LogService>();

            LogService.Log(LogLevel.Information, nameof(Program), null, "Press CTRL-C to exit");

            CocApiClient client = new CocApiClient(GetCocApiConfiguration());
            
            client.Log += Client_Log;

            client.PlayersCache.PlayerUpdated += PlayerUpdater_PlayerUpdated;

            client.ClansCacheUpdater.ClanUpdated += ClansCache_ClanUpdated;

            client.ClansCacheUpdater.ClanWarLogUpdated += ClansCache_ClanWarLogUpdated;

            client.PlayersApi.QueryResult += QueryResult;

            client.ClansApi.QueryResult += QueryResult;

            await client.PlayersCache.UpdateAsync("#29GPU9CUJ"); //squirrel man

            client.PlayersCache.Start();

            await client.ClansCache.AddAsync("#8J82PV0C"); //fysb unbuckled

            await client.ClansCache.AddAsync("#28RUGUYJU"); //devhls lab

            await client.ClansCache.AddAsync("#2C8V29YJ"); // russian clan

            client.ClansCacheUpdater.Start();

            //var b = a.Headers.First(h => h.Key == "Date");

            //var c = b.Value;

            //var d = c.First();

            //Console.WriteLine();

            //await client.AddClanAsync("#8J82PV0C");

            //await client.AddOrUpdateClanAsync("#8J82PV0C", false, false, false, false);

            //await client.RemoveVillage("#8J82PV0C");

            //await client.AddVillage("#8J82PV0C");

            //var war = await cocApiClient.Clans.GetCurrentWarAsync("#8J82PV0C");

            ////await Task.Delay(100);

            ////ClanWar? privateWar = await cocApiClient.Clans.GetCurrentWarAsync("#22G0JJR8");

            ////await Task.Delay(100);

            //var clan = await cocApiClient.Clans.GetClanAsync("#8J82PV0C");

            ////await Task.Delay(100);

            //var group = await cocApiClient.Clans.GetClanWarLeagueGroupAsync("#8J82PV0C");

            ////await Task.Delay(100);

            //var tag = group.Rounds.First().WarTags.First();

            //ClanWar? leagueWar = await cocApiClient.Clans.GetClanWarLeagueWarAsync(tag);

            ////await Task.Delay(100);

            //var log = await cocApiClient.Clans.GetClanWarLogAsync("#8J82PV0C");

            ////await Task.Delay(100);

            //var village = await cocApiClient.Players.GetPlayerAsync("#VC8VY8Y8");

            ////await Task.Delay(100);

            //LeagueList? league = await cocApiClient.LeaguesApi.GetLeaguesAsync();

            ////await Task.Delay(100);

            //LeagueSeasonList? season = await cocApiClient.LeaguesApi.GetLeagueSeasonsAsync(league.Items.Last().Id.ToString());

            ////await Task.Delay(100);

            //PlayerRankingList playerRanking = await cocApiClient.LeaguesApi.GetLeagueSeasonRankingsAsync(league.Items.Last().Id.ToString(), season.Items.First().Id);

            ////await Task.Delay(100);

            //var locations = await cocApiClient.LocationsApi.GetLocationsAsync();

            ////await Task.Delay(100);

            //var labels = await cocApiClient.LabelsApi.GetClanLabelsAsync();

            ////await Task.Delay(100);

            //var labels2 = await cocApiClient.LabelsApi.GetPlayerLabelsAsync();

            //var services = ConfigureServices();

            //LogService = services.GetRequiredService<LogService>();

            //LogService.Log(LogLevel.Information, nameof(Program), null, "Press CTRL-C to exit");

            ////ConfigureCocApi(services);

            ////services.GetRequiredService<EventHandlerService>();

            WaitForExit();

            //////await CleanupAsync(services);

            ////if (args == null) { }
        }

        private static Task ClansCache_ClanWarLogUpdated(object sender, ChangedEventArgs<ClanWarLog> e)
        {
            LogService.Log(LogLevel.Debug, nameof(Program), null, "War log updated");

            return Task.CompletedTask;
        }

        private static Task Client_Log(object sender, LogEventArgs log)
        {
            LogService.Log(LogLevel.Debug, log.Source, log.Method, log.Message);

            return Task.CompletedTask;
        }

        private static Task QueryResult(object sender, QueryResultEventArgs log)
        {
            string seconds = ((int)log.QueryResult.Stopwatch.Elapsed.TotalSeconds).ToString();           

            if (log.QueryResult is QueryException exception)
            {
                if (exception.Exception is ApiException apiException)
                    LogService.Log(LogLevel.Debug, sender.GetType().Name, seconds, log.QueryResult.EncodedUrl(), apiException.ErrorContent.ToString());
                else
                    LogService.Log(LogLevel.Debug, sender.GetType().Name, seconds, log.QueryResult.EncodedUrl(), exception.Exception.Message);
            }

            if (log.QueryResult is QuerySuccess)
                LogService.Log(LogLevel.Information, sender.GetType().Name, seconds, log.QueryResult.EncodedUrl());

            return Task.CompletedTask;
        }

        private static Task ClansCache_ClanUpdated(object sender, ChangedEventArgs<Clan> e)
        {
            LogService.Log(LogLevel.Debug, nameof(Program), null, "Clan updated");

            return Task.CompletedTask;
        }

        private static Task PlayerUpdater_PlayerUpdated(object sender, ChangedEventArgs<Player> e)
        {
            LogService.Log(LogLevel.Debug, nameof(Program), null, "Player updated");

            return Task.CompletedTask;
        }

        public static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<LogService>()
                .AddSingleton<CocApiClient_old>()
                //.AddSingleton(GetCocApiConfiguration)
                .AddSingleton<EventHandlerService>()
                .BuildServiceProvider();
        }

        private static void WaitForExit()
        {
            string input;

            do
            {
                input = Console.ReadLine();
            }
            while (input != "quit" && input != "exit" && input != "stop");
        }

        private static void ConfigureCocApi(IServiceProvider services)
        {
            //CocApi cocApi = services.GetRequiredService<CocApi>();

            //List<string> clans = new List<string>
            //{
            //    "#8J82PV0C",   // FYSB Unbuckled
            //    "#2C8V29YJ",   // Зеленоград
            //    "#22VCPLR98",  // LostMeta Power
            //    "#8RJJ0C0Y",   // Rising Asylum
            //    "#22G0JJR8",   // FYSB
            //    "#P989QU9P",   // Burlap Thongs
            //    "#YJC98R20",   // The hunters
            //    "#GGR2GU8Y",   // kerala kings 2
            //    "#R09C9RR0",   // Parathanon
            //};

            //cocApi.Clans.Queue(clans); //optionally use the ClanBuilder object and Build method to insert Clan object loaded from your database
            //                           //events will fire as if your program was never restarted

            //cocApi.Clans.QueueClanVillages = true; //events in cocApi.Villages will fire on ClanVillages in clans that are in the clan queue

            //cocApi.Wars.DownloadLeagueWars = DownloadLeagueWars.Auto; //league wars will download at the beginning of the month only

            ////cocApi.Villages.Queue("#00000"); //you may watch individual villages as well. this queue may take a long time if there are many villages

            //cocApi.Clans.StartQueue();

            //cocApi.Wars.StartQueue();  //this will watch all wars for all clans in the clan queue plus any war that you load with the Queue method

            ////cocApi.Villages.StartQueue(); // only start monitoring the objects you care about
        }

        private static CocApiConfiguration GetCocApiConfiguration()
        {
            CocApiConfiguration cocApiConfiguration = new CocApiConfiguration
            {
                //Do not hard code these values
                //Store them in a json file instead.

                //NumberOfUpdaters = 1,  //list of clans to be updated can be split across multiple tasks to process faster
                TimeToWaitForWebRequests = TimeSpan.FromSeconds(5), //how long to wait on an HTTP response before canceling the task
                TokenTimeOut = TimeSpan.FromSeconds(1), //how long to wait before reusing a token, helps us avoid a rate limit from sc

                //minimum time to consider an object valid
                //if you want updates as fast as sc will provide them
                //do not provide a value for time to live
                LeagueGroupTimeToLive = TimeSpan.FromHours(1),
                LeagueGroupNotFoundTimeToLive = TimeSpan.FromHours(1),
                //VillageTimeToLive = TimeSpan.FromHours(1)
            };

            cocApiConfiguration.Tokens.Add(File.ReadAllText(@"E:\Desktop\token.txt"));

            return cocApiConfiguration;
        }

        //private static async Task CleanupAsync(IServiceProvider services)
        //{
        //services.GetRequiredService<LogService>().Log(LogLevel.Information, nameof(Program), null, "Quiting, please wait...");

        //CocApi cocApi = services.GetRequiredService<CocApi>();

        //await cocApi.Clans.StopQueueAsync();

        //await cocApi.Wars.StopQueueAsync();

        //await cocApi.Villages.StopQueueAsync();

        //cocApi.Dispose();
        //}
    }
}