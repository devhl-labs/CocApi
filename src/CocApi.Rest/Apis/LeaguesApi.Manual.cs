using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;

namespace CocApi.Rest.Apis;

public partial class LeaguesApi
{
    partial void AfterFetchBuilderBaseLeague(ref bool suppressDefaultLog, IFetchBuilderBaseLeagueApiResponse apiResponseLocalVar, string leagueId)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }

    partial void AfterFetchCapitalLeague(ref bool suppressDefaultLog, IFetchCapitalLeagueApiResponse apiResponseLocalVar, string leagueId)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }

    partial void AfterFetchLeague(ref bool suppressDefaultLog, IFetchLeagueApiResponse apiResponseLocalVar, string leagueId)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }

    partial void AfterFetchLeagueSeasonRankings(ref bool suppressDefaultLog, IFetchLeagueSeasonRankingsApiResponse apiResponseLocalVar, string leagueId, string seasonId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }

    partial void AfterFetchLeagueSeasons(ref bool suppressDefaultLog, IFetchLeagueSeasonsApiResponse apiResponseLocalVar, string leagueId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }

    partial void AfterFetchWarLeague(ref bool suppressDefaultLog, IFetchWarLeagueApiResponse apiResponseLocalVar, string leagueId)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{leagueId}", leagueId));
    }
}
