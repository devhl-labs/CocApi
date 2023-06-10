using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;
using System;

#pragma warning disable CA1822 // Mark members as static

namespace CocApi.Rest.Apis
{
    public partial class PlayersApi
    {
        partial void OnErrorFetchPlayer(Exception exception, string pathFormat, string path, string playerTag)
        {
            Logger.LogError(exception, "There was an error fetching the player for playerTag: {playerTag}", playerTag);
        }

        partial void OnErrorVerifyToken(Exception exception, string pathFormat, string path, VerifyTokenRequest body, string playerTag)
        {
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
    }
}
