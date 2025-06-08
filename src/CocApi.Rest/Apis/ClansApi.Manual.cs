#pragma warning disable CA1822 // Mark members as static

using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

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







    partial void AfterFetchClan(ref bool suppressDefaultLog, IFetchClanApiResponse apiResponseLocalVar, string clanTag)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchCapitalRaidSeasons(ref bool suppressDefaultLog, IFetchCapitalRaidSeasonsApiResponse apiResponseLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchClanMembers(ref bool suppressDefaultLog, IFetchClanMembersApiResponse apiResponseLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchClanWarLeagueGroup(ref bool suppressDefaultLog, IFetchClanWarLeagueGroupApiResponse apiResponseLocalVar, string clanTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchClanWarLeagueWar(ref bool suppressDefaultLog, IFetchClanWarLeagueWarApiResponse apiResponseLocalVar, string warTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{warTag}", warTag));
    }

    partial void AfterFetchClanWarLog(ref bool suppressDefaultLog, IFetchClanWarLogApiResponse apiResponseLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchCurrentWar(ref bool suppressDefaultLog, IFetchCurrentWarApiResponse apiResponseLocalVar, string clanTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterSearchClans(ref bool suppressDefaultLog, ISearchClansApiResponse apiResponseLocalVar, Option<int> locationId, Option<int> minMembers, Option<int> maxMembers, Option<int> minClanPoints, Option<int> minClanLevel, Option<int> limit, Option<string> name, Option<string> warFrequency, Option<string> after, Option<string> before, Option<string> labelIds)
    {
        suppressDefaultLog = true;
        Logger.LogTrace("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
    }
}

#pragma warning restore CA1822 // Mark members as static
