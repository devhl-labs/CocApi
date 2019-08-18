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
                //, "#2C8V29YJ"
                //, "#22VCPLR98"
                //, "#8RJJ0C0Y"
            };

            cocApi.Monitor(clans, 1);

            cocApi.Monitor(true);

            await Task.Delay(-1);
        }

        private static void CocApi_WarIsAccessibleChanged(ICurrentWarAPIModel currentWarAPIModel)
        {
            Console.WriteLine($"War is accessible changed:{currentWarAPIModel.Flags.WarIsAccessible}");
        }

        private static void CocApi_NewWar(ClanAPIModel oldClan, ICurrentWarAPIModel currentWarAPIModel)
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
