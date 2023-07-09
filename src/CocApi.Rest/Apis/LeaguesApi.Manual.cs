using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;

namespace CocApi.Rest.Apis;

public partial class LeaguesApi
{
    partial void AfterFetchBuilderBaseLeague(ref bool suppressDefaultLog, ApiResponse<BuilderBaseLeague> apiResponseLocalVar, string leagueId)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }

    partial void AfterFetchCapitalLeague(ref bool suppressDefaultLog, ApiResponse<CapitalLeague> apiResponseLocalVar, string leagueId)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }

    partial void AfterFetchLeague(ref bool suppressDefaultLog, ApiResponse<League> apiResponseLocalVar, string leagueId)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }

    partial void AfterFetchLeagueSeasonRankings(ref bool suppressDefaultLog, ApiResponse<PlayerRankingList> apiResponseLocalVar, string leagueId, string seasonId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }

    partial void AfterFetchLeagueSeasons(ref bool suppressDefaultLog, ApiResponse<LeagueSeasonList> apiResponseLocalVar, string leagueId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }

    partial void AfterFetchWarLeague(ref bool suppressDefaultLog, ApiResponse<WarLeague> apiResponseLocalVar, string leagueId)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }
}
