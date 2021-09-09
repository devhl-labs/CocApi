using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Model;
using Microsoft.Extensions.Hosting;

namespace CocApi.Test
{
    public class TestService : IHostedService
    {
        public ClansClient ClansClient { get; }
        public PlayersClient PlayersClient { get; }
        public PlayersApi PlayersApi { get; }
        public LocationsApi LocationsApi { get; }
        public LeaguesApi LeaguesApi { get; }

        public TestService(ClansClient clansClient, PlayersClient playersClient, PlayersApi playersApi, LocationsApi locationsApi, LeaguesApi leaguesApi)
        {
            ClansClient = clansClient;
            PlayersClient = playersClient;
            PlayersApi = playersApi;
            LocationsApi = locationsApi;
            LeaguesApi = leaguesApi;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await SanityCheck();
            await AddTestItems();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task AddTestItems()
        {
            await PlayersClient.AddOrUpdateAsync("#29GPU9CUJ"); //squirrel man

            await ClansClient.AddOrUpdateAsync("#8J82PV0C", downloadMembers: false); //fysb unbuckled
            await ClansClient.AddOrUpdateAsync("#22G0JJR8", downloadMembers: false); //fysb
            await ClansClient.AddOrUpdateAsync("#28RUGUYJU", downloadMembers: false); //devhls lab
            await ClansClient.AddOrUpdateAsync("#2C8V29YJ", downloadMembers: false); // russian clan
            await ClansClient.AddOrUpdateAsync("#JYULPG28", downloadMembers: false); // inphase
            await ClansClient.AddOrUpdateAsync("#2P0YUY0L0", downloadMembers: false); // testing closed war log
            await ClansClient.AddOrUpdateAsync("#PJYPYG9P", downloadMembers: false); // war heads
            await ClansClient.AddOrUpdateAsync("#2900Y0PP2"); // crimine sas
        }

        private async Task SanityCheck()
        {
            var playerGlobalRankings = await LocationsApi.FetchPlayerRankingAsync("global");
            var playerVersusGlobalRankings = await LocationsApi.FetchPlayerVersusRankingAsync("global");
            var clanGlobalRankings = await LocationsApi.FetchClanRankingOrDefaultAsync("global");
            var clanGlobalVersusRankings = await LocationsApi.FetchClanVersusRankingAsync("global");
            var leagueList = await LeaguesApi.FetchWarLeaguesOrDefaultAsync();
            var playerToken = await PlayersApi.VerifyTokenResponseAsync("#29GPU9CUJ", new VerifyTokenRequest("a"));
        }
    }
}
