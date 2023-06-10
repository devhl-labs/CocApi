#pragma warning disable CA1822 // Mark members as static

using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;
using System;

namespace CocApi.Rest.BaseApis
{
    public partial class ClansApi
    {
        partial void OnErrorFetchClan(Exception exception, string pathFormat, string path, string clanTag)
        {
            Logger.LogError(exception, "There was an error fetching the clan for clanTag: {clanTag}", clanTag);
        }

        partial void OnErrorFetchClanMembers(Exception exception, string pathFormat, string path, string clanTag, int? limit, string? after, string? before)
        {
            Logger.LogError(exception, "There was an error fetching the clan members for clanTag: {clanTag}", clanTag);
        }

        partial void OnErrorFetchClanWarLeagueGroup(Exception exception, string pathFormat, string path, string clanTag, bool? realtime)
        {
            Logger.LogError(exception, "There was an error fetching the clan war league group for clanTag: {clanTag}", clanTag);
        }

        partial void OnErrorFetchClanWarLeagueWar(Exception exception, string pathFormat, string path, string warTag, bool? realtime)
        {
            Logger.LogError(exception, "There was an error fetching the clan war league war for warTag: {warTag}", warTag);
        }

        partial void OnErrorFetchClanWarLog(Exception exception, string pathFormat, string path, string clanTag, int? limit, string? after, string? before)
        {
            Logger.LogError(exception, "There was an error fetching the clan war log for clanTag: {clanTag}", clanTag);
        }

        partial void OnErrorFetchCurrentWar(Exception exception, string pathFormat, string path, string clanTag, bool? realtime)
        {
            Logger.LogError(exception, "There was an error fetching the current war for clanTag: {clanTag}", clanTag);
        }

        partial void FormatGetClan(ref string clanTag)
        {
            clanTag = Clash.FormatTag(clanTag);
        }

        partial void FormatGetCapitalRaidSeasons(ref string clanTag, ref int? limit, ref string? after, ref string? before)
        {
            clanTag = Clash.FormatTag(clanTag);
        }

        partial void FormatGetClanMembers(ref string clanTag, ref int? limit, ref string? after, ref string? before)
        {
            clanTag = Clash.FormatTag(clanTag);
        }

        partial void FormatGetClanWarLeagueGroup(ref string clanTag, ref bool? realtime)
        {
            clanTag = Clash.FormatTag(clanTag);
        }

        partial void FormatGetClanWarLeagueWar(ref string warTag, ref bool? realtime)
        {
            warTag = Clash.FormatTag(warTag);
        }

        partial void FormatGetClanWarLog(ref string clanTag, ref int? limit, ref string? after, ref string? before)
        {
            clanTag = Clash.FormatTag(clanTag);
        }

        partial void FormatGetCurrentWar(ref string clanTag, ref bool? realtime)
        {
            clanTag = Clash.FormatTag(clanTag);
        }
    }
}
