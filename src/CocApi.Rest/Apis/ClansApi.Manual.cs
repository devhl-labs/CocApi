#pragma warning disable CA1822 // Mark members as static

using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CocApi.Rest.Apis;

public partial class ClansApi
{
    partial void OnErrorFetchClan(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, string clanTag)
    {
        suppressDefaultLog = true;
        Logger.LogError(exception, "There was an error fetching the clan for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorFetchClanMembers(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogError(exception, "There was an error fetching the clan members for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorFetchClanWarLeagueGroup(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, string clanTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        Logger.LogError(exception, "There was an error fetching the clan war league group for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorFetchClanWarLeagueWar(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, string warTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        Logger.LogError(exception, "There was an error fetching the clan war league war for warTag: {warTag}", warTag);
    }

    partial void OnErrorFetchClanWarLog(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogError(exception, "There was an error fetching the clan war log for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorFetchCurrentWar(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, string clanTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        Logger.LogError(exception, "There was an error fetching the current war for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorFetchCapitalRaidSeasons(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogError(exception, "There was an error fetching the capital raid season for clanTag: {clanTag}", clanTag);
    }

    partial void OnErrorSearchClans(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, Option<int> locationId, Option<int> minMembers, Option<int> maxMembers, Option<int> minClanPoints, Option<int> minClanLevel, Option<int> limit, Option<string> name, Option<string> warFrequency, Option<string> after, Option<string> before, Option<string> labelIds)
    {
        suppressDefaultLog = true;
        Logger.LogError(exception, "There was an error searching for clans");
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







    partial void AfterFetchClan(ref bool suppressDefaultLog, ApiResponse<Clan> apiResponseLocalVar, string clanTag)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchCapitalRaidSeasons(ref bool suppressDefaultLog, ApiResponse<ClanCapitalRaidSeasons> apiResponseLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchClanMembers(ref bool suppressDefaultLog, ApiResponse<List<ClanMember>> apiResponseLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchClanWarLeagueGroup(ref bool suppressDefaultLog, ApiResponse<ClanWarLeagueGroup> apiResponseLocalVar, string clanTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchClanWarLeagueWar(ref bool suppressDefaultLog, ApiResponse<ClanWar> apiResponseLocalVar, string warTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{warTag}", warTag));
    }

    partial void AfterFetchClanWarLog(ref bool suppressDefaultLog, ApiResponse<ClanWarLog> apiResponseLocalVar, string clanTag, Option<int> limit, Option<string> after, Option<string> before)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterFetchCurrentWar(ref bool suppressDefaultLog, ApiResponse<ClanWar> apiResponseLocalVar, string clanTag, Option<bool> realtime)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{clanTag}", clanTag));
    }

    partial void AfterSearchClans(ref bool suppressDefaultLog, ApiResponse<ClanList> apiResponseLocalVar, Option<int> locationId, Option<int> minMembers, Option<int> maxMembers, Option<int> minClanPoints, Option<int> minClanLevel, Option<int> limit, Option<string> name, Option<string> warFrequency, Option<string> after, Option<string> before, Option<string> labelIds)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
    }
}

#pragma warning restore CA1822 // Mark members as static
