using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.IBaseApis;
using CocApi.Rest.Models;
using Microsoft.Extensions.Hosting;

namespace CocApi.Test;

public class TestService : IHostedService
{
    public CustomClansClient ClansClient { get; }
    public CustomPlayersClient PlayersClient { get; }
    public IPlayersApi PlayersApi { get; }
    public ILocationsApi LocationsApi { get; }
    public ILeaguesApi LeaguesApi { get; }
    public IClansApi ClansApi { get; }

    public TestService(
        CustomClansClient clansClient,
        CustomPlayersClient playersClient,
        IPlayersApi playersApi,
        ILocationsApi locationsApi,
        ILeaguesApi leaguesApi,
        IClansApi clansApi)
    {
        ClansClient = clansClient;
        PlayersClient = playersClient;
        PlayersApi = playersApi;
        LocationsApi = locationsApi;
        LeaguesApi = leaguesApi;
        ClansApi = clansApi;
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

        await ClansClient.AddOrUpdateAsync("#8J82PV0C"); //fysb unbuckled
        await ClansClient.AddOrUpdateAsync("#22G0JJR8"); //fysb
        await ClansClient.AddOrUpdateAsync("#28RUGUYJU"); //devhls lab
        await ClansClient.AddOrUpdateAsync("#2C8V29YJ"); // russian clan
        await ClansClient.AddOrUpdateAsync("#JYULPG28"); // inphase
        await ClansClient.AddOrUpdateAsync("#2P0YUY0L0"); // testing closed war log
        await ClansClient.AddOrUpdateAsync("#PJYPYG9P"); // war heads
        await ClansClient.AddOrUpdateAsync("#8PU9VR82"); // kronos
        await ClansClient.AddOrUpdateAsync("#2900Y0PP2"); // crimine sas
    }

    private async Task SanityCheck()
    {
        var clan = await ClansApi.FetchClanAsync("#29Y8PRCJR");
        var devhlsLab = await ClansApi.FetchClanAsync("#28RUGUYJU");
        var playerGlobalRankings = await LocationsApi.FetchPlayerRankingAsync("global");
        var playerVersusGlobalRankings = await LocationsApi.FetchPlayerVersusRankingAsync("global");
        var clanGlobalRankings = await LocationsApi.FetchClanRankingOrDefaultAsync("global");
        var clanGlobalVersusRankings = await LocationsApi.FetchClanVersusRankingAsync("global");
        var leagueList = await LeaguesApi.FetchWarLeaguesOrDefaultAsync();
        var playerToken = await PlayersApi.VerifyTokenAsync(new VerifyTokenRequest("a"), "#29GPU9CUJ");
        var warLog = await ClansApi.FetchClanWarLogAsync("#29Y8PRCJR");
        var clans = await ClansApi.SearchClansAsync(name: "fysb");
        ClanCapitalRaidSeasons a = await ClansApi.FetchCapitalRaidSeasonsAsync("#22G0JJR8");
        System.Console.WriteLine();
    }
}
