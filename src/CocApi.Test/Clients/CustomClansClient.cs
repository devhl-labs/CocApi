using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Cache.Services;
using CocApi.Model;
using Microsoft.Extensions.Options;

namespace CocApi.Test
{
    public class CustomClansClient : Cache.ClansClient
    {
        public CustomClansClient(
            CacheDbContextFactoryProvider dbContextOptions, 
            ClansApi clansApi, 
            Synchronizer synchronizer,
            ClanService clanMonitor,
            NewWarService newWarMonitor,
            NewCwlWarService newCwlWarMonitor,
            WarService warMonitor,
            CwlWarService cwlWarMonitor,
            IOptions<CacheOptions> options) 
            : base(clansApi, dbContextOptions, synchronizer, clanMonitor, newWarMonitor, newCwlWarMonitor, warMonitor, cwlWarMonitor, options)
        {
            ClanUpdated += OnClanUpdated;
            ClanWarAdded += OnClanWarAdded;
            ClanWarLogUpdated += OnClanWarLogUpdated;
            ClanWarUpdated += OnClanWarUpdated;
        }

        private Task OnClanUpdated(object sender, ClanUpdatedEventArgs e)
        {
            if (e.Stored == null)
                return Task.CompletedTask;

            List<Donation> donations = Clan.Donations(e.Stored, e.Fetched);

            if (donations.Count > 0)
                LogService.Log(LogLevel.Information, this.GetType().Name, "Clan updated" + donations.Count + " " + donations.Sum(d => d.Quanity));

            foreach (ClanMember member in Clan.ClanMembersLeft(e.Stored, e.Fetched))
                Console.WriteLine(member.Name + " left");

            foreach (ClanMember member in Clan.ClanMembersJoined(e.Stored, e.Fetched))
                Console.WriteLine(member.Name + " joined");

            return Task.CompletedTask;
        }

        private Task OnClanWarUpdated(object sender, ClanWarUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Information, this.GetType().Name, 
                $"War updated {e.Fetched.Clan.Tag} {e.Fetched.Opponent.Tag} {ClanWar.NewAttacks(e.Stored, e.Fetched).Count} new attack");

            return Task.CompletedTask;
        }

        private Task OnClanWarAdded(object sender, WarAddedEventArgs e)
        {
            LogService.Log(LogLevel.Information, this.GetType().Name, "New war");

            return Task.CompletedTask;
        }

        private Task OnClanWarLogUpdated(object sender, ClanWarLogUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Information, this.GetType().Name, "War log updated"); 

            return Task.CompletedTask;
        }
    }
}
