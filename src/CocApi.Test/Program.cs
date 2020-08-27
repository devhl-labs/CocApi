﻿using System;
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
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using CocApi.Cache.Models;

namespace CocApi.Test
{
    public class Client : CocApiClientBase
    {
        public Client(CocApiConfiguration cocApiConfiguration) : base(cocApiConfiguration)
        {
            
        }

        public override bool HasUpdated(Clan stored, Clan fetched)
        {
            return base.HasUpdated(stored, fetched);
        }

        public override TimeSpan TimeToLive(CachedPlayer cachedPlayer, ApiResponse<Player> apiResponse)
        {
            return TimeSpan.FromMinutes(1);
        }
    }

    class Program
    {
        public static LogService LogService { get; set; }

        public static async Task Main(string[] args)
        {
            var services = ConfigureServices();

            LogService = services.GetRequiredService<LogService>();

            LogService.Log(LogLevel.Information, nameof(Program), null, "Press CTRL-C to exit");

            Client client = new Client(GetCocApiConfiguration());
            
            client.Log += Client_Log;

            client.PlayersApi.QueryResult += QueryResult;
            client.ClansApi.QueryResult += QueryResult;

            client.PlayersCache.PlayerUpdated += PlayerUpdater_PlayerUpdated;

            client.ClansCache.ClanUpdated += ClansCache_ClanUpdated;
            client.ClansCache.ClanWarAdded += ClansCache_ClanWarAdded;
            client.ClansCache.ClanWarEndingSoon += ClansCache_ClanWarEndingSoon;
            client.ClansCache.ClanWarEndNotSeen += ClansCache_ClanWarEndNotSeen;
            client.ClansCache.ClanWarLeagueGroupUpdated += ClansCache_ClanWarLeagueGroupUpdated;
            client.ClansCache.ClanWarLogUpdated += ClansCache_ClanWarLogUpdated;
            client.ClansCache.ClanWarStartingSoon += ClansCache_ClanWarStartingSoon;
            client.ClansCache.ClanWarUpdated += ClansCache_ClanWarUpdated;


            await client.PlayersCache.UpdateAsync("#29GPU9CUJ"); //squirrel man

            client.PlayersCache.Start();

            await client.ClansCache.UpdateAsync("#8J82PV0C", downloadMembers: false); //fysb unbuckled
            await client.ClansCache.AddAsync("#22G0JJR8"); //fysb

            await client.ClansCache.AddAsync("#28RUGUYJU"); //devhls lab

            await client.ClansCache.AddAsync("#2C8V29YJ"); // russian clan

            client.ClansCache.Start();

            //var services = ConfigureServices();

            //LogService = services.GetRequiredService<LogService>();

            //LogService.Log(LogLevel.Information, nameof(Program), null, "Press CTRL-C to exit");

            ////ConfigureCocApi(services);

            ////services.GetRequiredService<EventHandlerService>();

            WaitForExit();

            //////await CleanupAsync(services);

            ////if (args == null) { }
        }

        private static Task ClansCache_ClanWarUpdated(object sender, ClanWarUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Debug, nameof(Program), null, "War updated " + ClanWar.NewAttacks(e.Stored, e.Fetched).Count);

            return Task.CompletedTask;
        }

        private static Task ClansCache_ClanWarStartingSoon(object sender, ClanWarEventArgs e)
        {
            LogService.Log(LogLevel.Debug, nameof(Program), null, "War starting soon");

            return Task.CompletedTask;
        }

        private static Task ClansCache_ClanWarEndNotSeen(object sender, ClanWarEventArgs e)
        {
            LogService.Log(LogLevel.Debug, nameof(Program), null, "War war end not seen");

            return Task.CompletedTask;
        }

        private static Task ClansCache_ClanWarEndingSoon(object sender, ClanWarEventArgs e)
        {
            LogService.Log(LogLevel.Debug, nameof(Program), null, "War ending soon");

            return Task.CompletedTask;
        }

        private static Task ClansCache_ClanWarAdded(object sender, ClanWarEventArgs e)
        {
            LogService.Log(LogLevel.Debug, nameof(Program), null, "New war");

            return Task.CompletedTask;
        }

        private static Task ClansCache_ClanWarLeagueGroupUpdated(object sender, ClanWarLeagueGroupUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Debug, nameof(Program), null, "Group updated");

            return Task.CompletedTask;
        }

        private static Task ClansCache_ClanWarLogUpdated(object sender, ClanWarLogUpdatedEventArgs e)
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

        private static Task ClansCache_ClanUpdated(object sender, ClanUpdatedEventArgs e)
        {
            var donations = Clan.Donations(e.Stored, e.Fetched);

            if (donations.Count > 0)            
                LogService.Log(LogLevel.Debug, nameof(Program), null, "Clan updated" + donations.Count + " " + donations.Sum(d => d.Quanity));            
            else
                LogService.Log(LogLevel.Debug, nameof(Program), null, "Clan updated");

            foreach(ClanMember member in Clan.ClanMembersLeft(e.Stored, e.Fetched))
                Console.WriteLine(member.Name + " left");

            foreach (ClanMember member in Clan.ClanMembersJoined(e.Stored, e.Fetched))
                Console.WriteLine(member.Name + " joined");

            return Task.CompletedTask;
        }

        private static Task PlayerUpdater_PlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Debug, nameof(Program), null, "Player updated");

            return Task.CompletedTask;
        }

        public static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<LogService>()
                .AddSingleton<CocApiClientBase>()
                //.AddSingleton(GetCocApiConfiguration)
                //.AddSingleton<EventHandlerService>()
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