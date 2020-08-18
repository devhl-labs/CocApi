using System;
using System.Collections.Generic;
using System.Linq;

using CocApi.Cache.Models;
using System.Threading.Tasks;
using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Wars;
using CocApi.Cache.Models.Villages;
using System.Collections.Immutable;
using CocApi.Cache;

namespace CocApi.Test
{
    public class EventHandlerService
    {
        private LogService LogService { get; }

        public EventHandlerService(LogService logger, CocApiClient_old cocApi)
        {
            LogService = logger;

            //cocApi.Clans.ClanChanged += Clans_ClanChanged;

            //cocApi.Wars.NewAttacks += Wars_NewAttacks;

            //cocApi.Clans.ClanDonation += Clans_ClanDonation;

            //cocApi.Clans.ClanVillageChanged += Clans_ClanVillageChanged;

            //cocApi.Clans.ClanVillagesJoined += Clans_ClanVillagesJoined;

            //cocApi.Clans.ClanVillagesLeft += Clans_ClanVillagesLeft;

            //cocApi.Log += CocApi_Log;
        }

        private Task Clans_ClanVillagesLeft(object sender, ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>> e)
        {
            LogService.Log(LogLevel.Information, nameof(EventHandlerService), null, "left");

            return Task.CompletedTask;
        }

        private Task Clans_ClanVillagesJoined(object sender, ChangedEventArgs<Clan, IReadOnlyList<ClanVillage>> e)
        {
            LogService.Log(LogLevel.Information, nameof(EventHandlerService), null, "joined");

            return Task.CompletedTask;
        }

        //private Task Clans_ClanVillageChanged(object sender, ChangedEventArgs<Clan, ClanVillage, ClanVillage> e)
        //{
        //    LogService.Log(LogLevel.Information, nameof(EventHandlerService), null, "clanvillage changed");

        //    return Task.CompletedTask;
        //}

        //private Task CocApi_Log(object sender, LogEventArgs log)
        //{
        //    //LogEventArgs may be cast to ClanEventArgs, CurrentWarEventArgs, ExceptionLogEventArgs, or VillageEventArgs
        //    //though you should not have to do this unless there is a problem

        //    //ignore the trace logs unless there is a problem
        //    if (log.LogLevel == devhl.CocApi.LogLevel.Trace)
        //        return Task.CompletedTask;

        //    LogService.Log(LogLevel.Information, log.Source, null, log.Message);

        //    return Task.CompletedTask;
        //}

        //private Task Wars_NewAttacks(object sender, ChangedEventArgs<CurrentWar, IReadOnlyList<Attack>> e)
        //{
        //    //e.Fetched refers to the newly downloaded object
        //    //e.Stored refers to the object stored in memory that is queued for download
        //    //the fetched item is generally provided when available

        //    LogService.Log(LogLevel.Information, nameof(EventHandlerService), null, $"{e.Value.Count()} new attacks in war between {e.Fetched.WarClans[0].Name} and {e.Fetched.WarClans[1].Name}");

        //    return Task.CompletedTask;
        //}

        private Task Clans_ClanChanged(object sender, ChangedEventArgs<Clan, Clan> e)
        {
            LogService.Log(LogLevel.Information, nameof(EventHandlerService), null, $"{e.Fetched.ClanTag} {e.Fetched.Name} changed event");

            return Task.CompletedTask;
        }

        private Task Clans_ClanDonation(object sender, DonationEventArgs e)
        {
            LogService.Log(LogLevel.Information, nameof(EventHandlerService), null, $"new donations in ${e.Fetched.ClanTag} {e.Fetched.Name}");

            return Task.CompletedTask;
        }
    }
}
