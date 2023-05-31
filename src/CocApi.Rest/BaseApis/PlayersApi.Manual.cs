using CocApi.Rest.Models;

#pragma warning disable CA1822 // Mark members as static

namespace CocApi.Rest.BaseApis
{
    public partial class PlayersApi
    {
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
