using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CocApiLibrary;
using CocApiLibrary.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace ClashOfClansConsoleTest
{
    class Program
    {
        private static readonly object logLock = new object();

        public static async Task Main(string[] args)
        {
            IList<string> tokens = new List<string>
            {
                File.ReadAllText(@"E:\Desktop\token.txt")
            };

            //Configuration configuration = new Configuration();

            //configuration.NumberOfUpdaters = 1;

            //configuration.TimeToWaitForWebRequests = 6000;

            //configuration.TokenTimeOutMilliseconds = 3000;

            //CreateHostBuilder

            //ILoggerFactory loggerFactory = new loggerf

            //ILogger logger = new Logger<Program>()

            CocApi cocApi = new CocApi(tokens, logger: LogMessages);

            var village = await cocApi.GetVillageAsync("#20LRPJG2U");

            //var clan = await cocApi.GetClanAsync("#8J82PV0C");

            var clan2 = await cocApi.GetClanAsync("#2C8V29YJ");

            var currentwar = await cocApi.GetCurrentWarAsync("#8RJJ0C0Y");

            var leaguegroup = await cocApi.GetLeagueGroupAsync("#8J82PV0C");

            ////var clans = await cocApi.GetClansAsync("the");

            //var leaguewar = await cocApi.GetLeagueWarAsync("#2PC9VR9P0");

            //var wars = await cocApi.GetWarLogAsync("#8J82PV0C");

            //var test = await cocApi.GetClansAsync("a");




            cocApi.ClanChanged += CocApi_ClanChanged;

            cocApi.IsAvailableChanged += CocApi_IsAvailableChanged;

            cocApi.MembersJoined += CocApi_MembersJoined;

            cocApi.ClanBadgeUrlChanged += CocApi_ClanBadgeUrlChanged;

            cocApi.ClanLocationChanged += CocApi_ClanLocationChanged;

            cocApi.NewAttacks += CocApi_NewAttacks;

            cocApi.ClanPointsChanged += CocApi_ClanPointsChanged;

            cocApi.ClanVersusPointsChanged += CocApi_ClanVersusPointsChanged;

            cocApi.NewWar += CocApi_NewWar;

            cocApi.WarIsAccessibleChanged += CocApi_WarIsAccessibleChanged;

            List<string> clans = new List<string>
            {
                "#8J82PV0C"
                , "#2C8V29YJ"
                , "#22VCPLR98"
                , "#8RJJ0C0Y"
            };

            cocApi.WatchClans(clans);

            cocApi.BeginUpdatingClans();

            //await Task.Delay(10000);

            ////await cocApi.StopUpdatingClans();

            cocApi.DownloadLeagueWars = true;

            cocApi.DownloadVillages = true;

            //await Task.Delay(10000);

            //cocApi.UpdateClan("#2C8V29YJ");

            //await Task.Delay(10000);

            //cocApi.DownloadLeagueWars = true;

            await Task.Delay(-1);
        }

        //public static IHostBuilder CreateHostBuilder(string[] args)
        //{
        //    return Host.CreateDefaultBuilder(args).ConfigureHostConfiguration(webBuilder =>
        //    {
        //        webBuilder.UseStartup<Program>();
        //    });
        //}


        public static Task LogMessages(LogMessage logMessage)
        {
            lock (logLock)
            {
                PrintLogTitle(logMessage);

                Console.WriteLine(logMessage.ToString());
            }
            //Console.WriteLine(logMessage.Message);

            return Task.CompletedTask;
        }

        private static void ResetConsoleColor()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void PrintLogTitle(LogMessage logMessage)
        {
            switch (logMessage.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[crit] ");
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[dbug] ");
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.Write("[err ]");
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[info] ");
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[verb] ");
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[warn] ");
                    break;
            }

            ResetConsoleColor();
        }

        private static void CocApi_WarIsAccessibleChanged(ICurrentWarAPIModel currentWarAPIModel)
        {
            Console.WriteLine($"War is accessible changed:{currentWarAPIModel.Flags.WarIsAccessible}");
        }

        private static void CocApi_NewWar(ICurrentWarAPIModel currentWarAPIModel)
        {
            Console.WriteLine($"New War: {currentWarAPIModel.WarID}");
        }

        private static void CocApi_ClanVersusPointsChanged(ClanAPIModel oldClan, int newClanVersusPoints)
        {
            Console.WriteLine($"{oldClan.Tag} {oldClan.Name} new clan versus points: {newClanVersusPoints}");
        }

        private static void CocApi_ClanPointsChanged(ClanAPIModel oldClan, int newClanPoints)
        {
            Console.WriteLine($"{oldClan.Tag} {oldClan.Name} new clan points: {newClanPoints}");
        }

        private static void CocApi_ClanLocationChanged(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            Console.WriteLine(newClan.Location?.Name);
        }

        private static void CocApi_ClanBadgeUrlChanged(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            Console.WriteLine(newClan.BadgeUrls?.Large);
        }

        private static void CocApi_ClanChanged(ClanAPIModel oldClan, ClanAPIModel newClan)
        {
            Console.WriteLine($"{oldClan.Tag} {oldClan.Name} changed.");
        }

        private static void CocApi_NewAttacks(ICurrentWarAPIModel currentWarAPIModel, List<AttackAPIModel> attackAPIModels)
        {
            Console.WriteLine($"new attacks: {attackAPIModels.Count()}");
        }

        private static void CocApi_MembersJoined(ClanAPIModel clanAPIModel, List<MemberListAPIModel> memberListAPIModels)
        {
            Console.WriteLine($"{memberListAPIModels.Count()} members joined.");
        }

        private static void CocApi_IsAvailableChanged(bool isAvailable)
        {
            Console.WriteLine($"CocApi isAvailable: {isAvailable}");
        }
    }
}
