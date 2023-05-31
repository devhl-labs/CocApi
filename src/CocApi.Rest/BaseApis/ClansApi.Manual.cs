#pragma warning disable CA1822 // Mark members as static

namespace CocApi.Rest.BaseApis
{
    public partial class ClansApi
    {
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
