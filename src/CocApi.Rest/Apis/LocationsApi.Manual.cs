using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;

namespace CocApi.Rest.Apis;

public partial class LocationsApi
{
    partial void AfterFetchClanBuilderBaseRanking(ref bool suppressDefaultLog, ApiResponse<ClanBuilderBaseRankingList> apiResponseLocalVar, string locationId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }

    partial void AfterFetchClanCapitalRanking(ref bool suppressDefaultLog, ApiResponse<ClanCapitalRankingObject> apiResponseLocalVar, string locationId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }

    partial void AfterFetchClanRanking(ref bool suppressDefaultLog, ApiResponse<ClanRankingList> apiResponseLocalVar, string locationId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }

    partial void AfterFetchLocation(ref bool suppressDefaultLog, ApiResponse<Location> apiResponseLocalVar, string locationId)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }

    partial void AfterFetchPlayerBuilderBaseRanking(ref bool suppressDefaultLog, ApiResponse<PlayerBuilderBaseRankingList> apiResponseLocalVar, string locationId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }

    partial void AfterFetchPlayerRanking(ref bool suppressDefaultLog, ApiResponse<PlayerRankingList> apiResponseLocalVar, string locationId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }
}
