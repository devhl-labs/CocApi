using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;

namespace CocApi.Rest.Apis;

public partial class LocationsApi
{
    partial void AfterFetchClanBuilderBaseRanking(ref bool suppressDefaultLog, IFetchClanBuilderBaseRankingApiResponse apiResponseLocalVar, string locationId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }

    partial void AfterFetchClanCapitalRanking(ref bool suppressDefaultLog, IFetchClanCapitalRankingApiResponse apiResponseLocalVar, string locationId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }

    partial void AfterFetchClanRanking(ref bool suppressDefaultLog, IFetchClanRankingApiResponse apiResponseLocalVar, string locationId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }

    partial void AfterFetchLocation(ref bool suppressDefaultLog, IFetchLocationApiResponse apiResponseLocalVar, string locationId)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }

    partial void AfterFetchPlayerBuilderBaseRanking(ref bool suppressDefaultLog, IFetchPlayerBuilderBaseRankingApiResponse apiResponseLocalVar, string locationId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }

    partial void AfterFetchPlayerRanking(ref bool suppressDefaultLog, IFetchPlayerRankingApiResponse apiResponseLocalVar, string locationId, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{locationId}", locationId));
    }
}
