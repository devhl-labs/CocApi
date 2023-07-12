using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;
using System;

#pragma warning disable CA1822 // Mark members as static

namespace CocApi.Rest.Apis;

public partial class PlayersApi
{
    partial void OnErrorFetchPlayer(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, string playerTag)
    {
        suppressDefaultLog = true;
        Logger.LogError(exception, "There was an error fetching the player for playerTag: {playerTag}", playerTag);
    }

    partial void OnErrorVerifyToken(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, VerifyTokenRequest body, string playerTag)
    {
        suppressDefaultLog = true;
        Logger.LogError(exception, "There was an error fetching the token for playerTag: {playerTag}", playerTag);
    }

    partial void FormatGetPlayer(ref string playerTag)
    {
        playerTag = Clash.FormatTag(playerTag);
    }

    partial void FormatVerifyToken(VerifyTokenRequest body, ref string playerTag)
    {
        playerTag = Clash.FormatTag(playerTag);
    }

    partial void AfterFetchPlayer(ref bool suppressDefaultLog, ApiResponse<Player> apiResponseLocalVar, string playerTag)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{playerTag}", playerTag));
    }

    partial void AfterVerifyToken(ref bool suppressDefaultLog, ApiResponse<VerifyTokenResponse> apiResponseLocalVar, VerifyTokenRequest body, string playerTag)
    {
        suppressDefaultLog = true;
        Logger.LogInformation("{elapsed,-9} | {status} | {url}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path.Replace("{playerTag}", playerTag));
    }
}

#pragma warning restore CA1822 // Mark members as static
