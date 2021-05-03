using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CocApi.Test
{
    public class ClansClient : ClansClientBase
    {
        private readonly PlayersClientBase _playersCache;
        private readonly LocationsApi _locationsApi;
        private readonly LeaguesApi _leaguesApi;
        private readonly PlayersApi _playersApi;

        public ClansClient(
            CacheDbContextFactoryProvider dbContextOptions, 
            ClansApi clansApi, 
            PlayersClientBase playersCache, 
            LocationsApi locationsApi, 
            LeaguesApi leaguesApi,
            PlayersApi playersApi, 
            IOptions<ClanClientOptions> options) 
            : base(clansApi, playersCache, playersApi, dbContextOptions, options)
        {
            _playersCache = playersCache;
            _locationsApi = locationsApi;
            _leaguesApi = leaguesApi;
            _playersApi = playersApi;

            ClanUpdated += OnClanUpdated;
            ClanWarAdded += OnClanWarAdded;
            ClanWarLogUpdated += OnClanWarLogUpdated;
            ClanWarUpdated += OnClanWarUpdated;
        }

        public ClansClient(ClansApi clansApi, PlayersClientBase playersClient, PlayersApi playersApi, 
            CacheDbContextFactoryProvider provider, IOptions<ClanClientOptions> options) 
            : base(clansApi, playersClient, playersApi, provider, options)
        {
        }

        public new async Task StartAsync(CancellationToken cancellationToken)
        {
            // add some dummy data to the database
            await AddInitialItems();

            // import old cache data from CocApi.Cache verison 1.4
            await ImportDataToVersion2(@"Data Source=E:\repos\CocApi\src\CocApi.Test\bin\Debug\net5.0\mb-CocApi.Cache.sqlite");

            //await SanityCheck();

            await base.StartAsync(cancellationToken);
        }

        private async Task AddInitialItems()
        {
            await _playersCache.AddOrUpdateAsync("#29GPU9CUJ"); //squirrel man

            await AddOrUpdateAsync("#8J82PV0C", downloadMembers: false); //fysb unbuckled
            await AddOrUpdateAsync("#22G0JJR8", downloadMembers: false); //fysb
            await AddOrUpdateAsync("#28RUGUYJU", downloadMembers: false); //devhls lab
            await AddOrUpdateAsync("#2C8V29YJ", downloadMembers: false); // russian clan
            await AddOrUpdateAsync("#JYULPG28", downloadMembers: false); // inphase
            await AddOrUpdateAsync("#2P0YUY0L0", downloadMembers: false); // testing closed war log
            await AddOrUpdateAsync("#PJYPYG9P", downloadMembers: false); // war heads
            await AddOrUpdateAsync("#2900Y0PP2"); // crimine sas
        }

        private async Task SanityCheck()
        {
            var playerGlobalRankings = await _locationsApi.FetchPlayerRankingAsync("global");
            var playerVersusGlobalRankings = await _locationsApi.FetchPlayerVersusRankingAsync("global");
            var clanGlobalRankings = await _locationsApi.FetchClanRankingOrDefaultAsync("global");
            var clanGlobalVersusRankings = await _locationsApi.FetchClanVersusRankingAsync("global");

            var leagueList = await _leaguesApi.FetchWarLeaguesOrDefaultAsync();

            var playerToken = await _playersApi.VerifyTokenResponseAsync("#29GPU9CUJ", new VerifyTokenRequest("a"));
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

        protected override bool HasUpdated(Clan? stored, Clan fetched)
        {
            // there are many properties in a clan
            // a change in any one will cause an update to the database
            // return false if you dont care about the changes seen
            // this will avoid unnecessary database updates

            return base.HasUpdated(stored, fetched);
        }
    }
}
