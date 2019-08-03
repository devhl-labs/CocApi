using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            CocApi cocApi = new CocApi(tokens, 3000, 60000, Enums.VerbosityType.None);

            //var village = await cocApi.GetVillageAsync("#20LRPJG2U");

            ////var clan = await cocApi.GetClanAsync("#8J82PV0C");

            //var clan2 = await cocApi.GetClanAsync("#2C8V29YJ");

            //var currentwar = await cocApi.GetCurrentWarAsync("#8RJJ0C0Y");

            ////var leaguegroup = await cocApi.GetLeagueGroupAsync("#8J82PV0C");

            ////var clans = await cocApi.GetClansAsync("the");

            //var leaguewar = await cocApi.GetLeagueWarAsync("#2PC9VR9P0");

            //var wars = await cocApi.GetWarLogAsync("#8J82PV0C");



            List<string> clans = new List<string>();

            clans.Add("#8J82PV0C");

            cocApi.MonitorClans(clans, 1);

            cocApi.Monitor(true);

            await Task.Delay(10000);

            ClanAPIModel clan = await cocApi.GetClanAsync("#8J82PV0C");

            cocApi.ClanChanged += CocApi_ClanChanged;

            cocApi.IsAvailableChanged += CocApi_IsAvailableChanged;

            cocApi.MembersJoined += CocApi_MembersJoined;

            clan.BadgeUrls.Large = "test";

            clan.Name = "test";

            clan.Members = new List<MemberListAPIModel>();

            Console.WriteLine("");

            await Task.Delay(-1);
        }

        private static void CocApi_MembersJoined(ClanAPIModel clanAPIModel, List<MemberListAPIModel> memberListAPIModels)
        {
            Console.WriteLine($"{memberListAPIModels.Count()} members joined.");
        }

        private static void CocApi_ClanChanged(ClanAPIModel clanAPIModel)
        {
            Console.WriteLine("clan changed");
        }

        private static void CocApi_IsAvailableChanged(bool isAvailable)
        {
            throw new NotImplementedException();
        }
    }
}
