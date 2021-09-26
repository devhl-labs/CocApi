using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Cache.Services;
using CocApi.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Test
{
    public class CustomClansClient : ClansClient
    {
        public CustomClansClient(
            ILogger<CustomClansClient> logger,
            IServiceScopeFactory scopeFactory,
            ClansApi clansApi, 
            Synchronizer synchronizer,
            IPerpetualExecution<object>[] perpetualServices,
            IOptions<CacheOptions> options) 
            : base(logger, clansApi, scopeFactory, synchronizer, perpetualServices, options)
        {
            ClanUpdated += OnClanUpdated;
            ClanWarAdded += OnClanWarAdded;
            ClanWarUpdated += OnClanWarUpdated;
        }

        private Task OnClanUpdated(object sender, ClanUpdatedEventArgs e)
        {
            if (e.Stored == null)
                return Task.CompletedTask;

            List<Donation> donations = Clan.Donations(e.Stored, e.Fetched);

            if (donations.Count > 0)
                Logger.LogInformation("{0} troops donated in {1} {2}", donations.Sum(d => d.Quanity), e.Fetched.Tag, e.Fetched.Name);

            foreach (ClanMember member in Clan.ClanMembersLeft(e.Stored, e.Fetched))
                Logger.LogInformation("{0} {1} left clan {2} {3}", member.Tag, member.Name, e.Fetched.Tag, e.Fetched.Name);

            foreach (ClanMember member in Clan.ClanMembersJoined(e.Stored, e.Fetched))
                Logger.LogInformation("{0} {1} joined clan {2} {3}", member.Tag, member.Name, e.Fetched.Tag, e.Fetched.Name);

            return Task.CompletedTask;
        }

        private Task OnClanWarUpdated(object sender, ClanWarUpdatedEventArgs e)
        {
            Logger.LogInformation("{0} new attacks between {1} vs {2}.", ClanWar.NewAttacks(e.Stored, e.Fetched).Count, e.Fetched.Clan.Tag, e.Fetched.Opponent.Tag);

            return Task.CompletedTask;
        }

        private Task OnClanWarAdded(object sender, WarAddedEventArgs e)
        {
            Logger.LogInformation("New war between {0} and {1}.", e.War.Clan.Tag, e.War.Opponent.Tag);

            return Task.CompletedTask;
        }
    }
}
