using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CocApiLibrary;
using CocApiLibrary.Models;

namespace ClashOfClansConsoleTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IList<string> tokens = new List<string>
            {
                File.ReadAllText(@"E:\Desktop\token.txt")
            };

            ClashOfClans cocApi = new ClashOfClans(tokens, 3000, 60000, Enums.VerbosityType.None);

            //var clan = await cocApi.GetClanAsync("#a");

            //var village = await cocApi.GetVillageAsync("#20LRPJG2U");

            var clan = await cocApi.GetClanAsync("#8J82PV0C");

            //var currentwar = await cocApi.GetCurrentWarAsync("#8RJJ0C0Y");

            //var leaguegroup = await cocApi.GetLeagueGroupAsync("#929YJPYJ");

            //await cocApi.GetClansAsync("t");

            //await cocApi.GetLeagueWarAsync("#2PC9VR9P0");

            //await cocApi.GetWarLogAsync("#8J82PV0C");

            //Console.WriteLine(currentwar.PreparationStartTime.ToDateTime());

            Console.WriteLine("");

        }
    }
}
