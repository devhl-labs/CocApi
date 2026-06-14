#pragma warning disable CA1822 // Mark members as static

using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace CocApi.Rest.Apis;

public partial class ClansApi
{
    partial void OnErrorFetchClan(ref bool suppressDefaultLogLocalVar, Exception exceptionLocalVar, string pathFormatLocalVar, string pathLocalVar, string clanTag)
    {
        suppressDefaultLogLocalVar = true;
        Logger.LogError(exceptionLocalVar, "There was an error fetching the clan for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorFetchClanMembers(ref bool suppressDefaultLogLocalVar, Exception exceptionLocalVar, string pathFormatLocalVar, string pathLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLogLocalVar = true;
        Logger.LogError(exceptionLocalVar, "There was an error fetching the clan members for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorFetchClanWarLeagueGroup(ref bool suppressDefaultLogLocalVar, Exception exceptionLocalVar, string pathFormatLocalVar, string pathLocalVar, string clanTag, Option<bool> realtime)
    {
        suppressDefaultLogLocalVar = true;
        Logger.LogError(exceptionLocalVar, "There was an error fetching the clan war league group for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorFetchClanWarLeagueWar(ref bool suppressDefaultLogLocalVar, Exception exceptionLocalVar, string pathFormatLocalVar, string pathLocalVar, string warTag, Option<bool> realtime)
    {
        suppressDefaultLogLocalVar = true;
        Logger.LogError(exceptionLocalVar, "There was an error fetching the clan war league war for warTag: {warTag}", warTag);
    }

    partial void OnErrorFetchClanWarLog(ref bool suppressDefaultLogLocalVar, Exception exceptionLocalVar, string pathFormatLocalVar, string pathLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLogLocalVar = true;
        Logger.LogError(exceptionLocalVar, "There was an error fetching the clan war log for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorFetchCurrentWar(ref bool suppressDefaultLogLocalVar, Exception exceptionLocalVar, string pathFormatLocalVar, string pathLocalVar, string clanTag, Option<bool> realtime)
    {
        suppressDefaultLogLocalVar = true;
        Logger.LogError(exceptionLocalVar, "There was an error fetching the current war for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorFetchCapitalRaidSeasons(ref bool suppressDefaultLogLocalVar, Exception exceptionLocalVar, string pathFormatLocalVar, string pathLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLogLocalVar = true;
        Logger.LogError(exceptionLocalVar, "There was an error fetching the capital raid season for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorSearchClans(ref bool suppressDefaultLogLocalVar, Exception exceptionLocalVar, string pathFormatLocalVar, string pathLocalVar, Option<int> locationId, Option<int> minMembers, Option<int> maxMembers, Option<int> minClanPoints, Option<int> minClanLevel, Option<int> limit, Option<string> name, Option<string> warFrequency, Option<string> after, Option<string> before, Option<string> labelIds)
    {
        suppressDefaultLogLocalVar = true;
        Logger.LogError(exceptionLocalVar, "There was an error searching for clans");
    }

    partial void FormatGetClan(ref string clanTag)
    {
        clanTag = Clash.FormatTag(clanTag);
    }

    partial void FormatGetCapitalRaidSeasons(ref string clanTag, ref Option<int> limit, ref Option<string> after, ref Option<string> before)
    {
        clanTag = Clash.FormatTag(clanTag);
    }

    partial void FormatGetClanMembers(ref string clanTag, ref Option<int> limit, ref Option<string> after, ref Option<string> before)
    {
        clanTag = Clash.FormatTag(clanTag);
    }

    partial void FormatGetClanWarLeagueGroup(ref string clanTag, ref Option<bool> realtime)
    {
        clanTag = Clash.FormatTag(clanTag);
    }

    partial void FormatGetClanWarLeagueWar(ref string warTag, ref Option<bool> realtime)
    {
        warTag = Clash.FormatTag(warTag);
    }

    partial void FormatGetClanWarLog(ref string clanTag, ref Option<int> limit, ref Option<string> after, ref Option<string> before)
    {
        clanTag = Clash.FormatTag(clanTag);
    }

    partial void FormatGetCurrentWar(ref string clanTag, ref Option<bool> realtime)
    {
        clanTag = Clash.FormatTag(clanTag);
    }







    /// <param name="warnOn403">
    /// <c>true</c>  → 403 is unexpected (bad token / IP mismatch): log Warning with TokenSuffix.<br/>
    /// <c>false</c> → 403 may be legitimate (e.g. private war log)
    /// </param>
    private void LogAfterFetch(IApiResponse apiResponse, string url, bool warnOn403 = true)
    {
        Logger.LogTrace("{elapsed,-9} | {status} | {url}",
            (apiResponse.DownloadedAt - apiResponse.RequestedAt).TotalSeconds,
            apiResponse.StatusCode,
            url);

        if (warnOn403 && apiResponse.StatusCode == HttpStatusCode.Forbidden)
        {
            var tokenSuffix = (apiResponse as ApiResponse)?.RequestTokenSuffix ?? "unknown";
            Logger.LogWarning(
                "403 Forbidden | Url={Url} | TokenSuffix={TokenSuffix} | Check: all API tokens must have the server IP in their allowlist at developer.clashofclans.com",
                url,
                tokenSuffix);
        }
    }

    partial void AfterFetchClan(ref bool suppressDefaultLog, IFetchClanApiResponse apiResponseLocalVar, string clanTag)
    {
        suppressDefaultLog = true;
        LogAfterFetch(apiResponseLocalVar, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchCapitalRaidSeasons(ref bool suppressDefaultLog, IFetchCapitalRaidSeasonsApiResponse apiResponseLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        LogAfterFetch(apiResponseLocalVar, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchClanMembers(ref bool suppressDefaultLog, IFetchClanMembersApiResponse apiResponseLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        LogAfterFetch(apiResponseLocalVar, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchClanWarLeagueGroup(ref bool suppressDefaultLog, IFetchClanWarLeagueGroupApiResponse apiResponseLocalVar, string clanTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        LogAfterFetch(apiResponseLocalVar, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchClanWarLeagueWar(ref bool suppressDefaultLog, IFetchClanWarLeagueWarApiResponse apiResponseLocalVar, string warTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        LogAfterFetch(apiResponseLocalVar, apiResponseLocalVar.Path.Replace("{warTag}", warTag));
    }

    partial void AfterFetchClanWarLog(ref bool suppressDefaultLog, IFetchClanWarLogApiResponse apiResponseLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        // 403 here typically means the war log is private — not a token issue
        LogAfterFetch(apiResponseLocalVar, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag), warnOn403: false);
    }

    partial void AfterFetchCurrentWar(ref bool suppressDefaultLog, IFetchCurrentWarApiResponse apiResponseLocalVar, string clanTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        // 403 here typically means the war log is private — not a token issue
        LogAfterFetch(apiResponseLocalVar, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag), warnOn403: false);
    }

    partial void AfterSearchClans(ref bool suppressDefaultLog, ISearchClansApiResponse apiResponseLocalVar, Option<int> locationId, Option<int> minMembers, Option<int> maxMembers, Option<int> minClanPoints, Option<int> minClanLevel, Option<int> limit, Option<string> name, Option<string> warFrequency, Option<string> after, Option<string> before, Option<string> labelIds)
    {
        suppressDefaultLog = true;
        LogAfterFetch(apiResponseLocalVar, apiResponseLocalVar.Path);
    }
}

#pragma warning restore CA1822 // Mark members as static
