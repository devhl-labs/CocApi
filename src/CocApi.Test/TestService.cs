using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.Client;
using CocApi.Rest.IApis;
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
        ApiResponse<Clan> clanResponse = await ClansApi.FetchClanAsync("#29Y8PRCJR");
        Clan? clan = clanResponse.AsModel();

        ApiResponse<Clan> devhlsLabResponse = await ClansApi.FetchClanAsync("#28RUGUYJU");
        Clan? devhlsLab = devhlsLabResponse.AsModel();

        ApiResponse<PlayerRankingList> playerGlobalRankingsResponse = await LocationsApi.FetchPlayerRankingAsync("global");
        PlayerRankingList? playerGlobalRankings = playerGlobalRankingsResponse.AsModel();

        // ApiResponse<PlayerBuilderBaseRankingList> playerVersusGlobalRankingsResponse = await LocationsApi.FetchPlayerBuilderBaseRankingAsync("global"); // this endpoint is broken
        //PlayerBuilderBaseRankingList playerVersusGlobalRankings = playerVersusGlobalRankingsResponse.AsModel();

        ApiResponse<ClanRankingList> clanGlobalRankingsResponse = await LocationsApi.FetchClanRankingAsync("global");

        ApiResponse<ClanBuilderBaseRankingList> clanGlobalVersusRankingsResponse = await LocationsApi.FetchClanBuilderBaseRankingAsync("global");
        ClanBuilderBaseRankingList? clanGlobalVersusRanking = clanGlobalVersusRankingsResponse.AsModel();

        ApiResponse<WarLeagueList> leagueListResponse = await LeaguesApi.FetchWarLeaguesAsync();
        WarLeagueList? leagueList = leagueListResponse.AsModel();

        ApiResponse<VerifyTokenResponse> playerTokenResponse = await PlayersApi.VerifyTokenAsync(new VerifyTokenRequest("a"), "#29GPU9CUJ");
        VerifyTokenResponse? playerToken = playerTokenResponse.AsModel();

        ApiResponse<ClanWarLog> warLogResponse = await ClansApi.FetchClanWarLogAsync("#29Y8PRCJR");
        ClanWarLog? warLog = warLogResponse.AsModel();

        ApiResponse<ClanList> clansResponse = await ClansApi.SearchClansAsync(name: new Option<string>("fysb"));
        ClanList? clans = clansResponse.AsModel();

        ApiResponse<ClanCapitalRaidSeasons> clanCapitalRaidSeasonsResponse = await ClansApi.FetchCapitalRaidSeasonsAsync("#22G0JJR8");
        ClanCapitalRaidSeasons? clanCapitalRaidSeasons = clanCapitalRaidSeasonsResponse.AsModel();

        ApiResponse<ClanCapitalRankingObject> clanCapitalRankingResponse = await LocationsApi.FetchClanCapitalRankingAsync("global");
        ClanCapitalRankingObject? clanCapitalRanking = clanCapitalRankingResponse.AsModel();

        ApiResponse<CapitalLeagueObject> capitalLeaguesResponse = await LeaguesApi.FetchCapitalLeaguesAsync();
        CapitalLeagueObject? capitalLeagues = capitalLeaguesResponse.AsModel();

        ApiResponse<CapitalLeague> capitalLeagueResponse = await LeaguesApi.FetchCapitalLeagueAsync("85000018");
        CapitalLeague? capitalLeague = capitalLeagueResponse.AsModel();

        ApiResponse<BuilderBaseLeagueList> builderBaseLeaguesResponse = await LeaguesApi.FetchBuilderBaseLeaguesAsync();
        BuilderBaseLeagueList? builderBaseLeagues = builderBaseLeaguesResponse.AsModel();

        ApiResponse<BuilderBaseLeague> builderBaseLeagueResponse = await LeaguesApi.FetchBuilderBaseLeagueAsync(builderBaseLeaguesResponse.AsModel()!.Items.First().Id.ToString());
        BuilderBaseLeague? builderBaseLeague = builderBaseLeagueResponse.AsModel();

        System.Console.WriteLine("Done sanity check.");
    }
}
